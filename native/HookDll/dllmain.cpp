// HookDll.dll
// Injects into all GUI processes via SetWindowsHookEx(WH_CBT, ...) and
// hooks RegisterHotKey via IAT patching to log global hotkey registrations.
//
// Build: x64 Release, Visual Studio 2022, Windows SDK 10.0
//
// Protocol: shared memory-mapped file named "Global\\HotkeyInspectorSHM"
// Record format (32 bytes):
//   offset  0: DWORD processId
//   offset  4: DWORD threadId
//   offset  8: DWORD hotkeyId
//   offset 12: DWORD fsModifiers
//   offset 16: DWORD vk
//   offset 20: DWORD timestamp
//   offset 24: CHAR  action  ('R'=register)
//   offset 25: CHAR  padding[7]

#include <windows.h>
#include <psapi.h>

#pragma comment(lib, "psapi.lib")

// ---------------------------------------------------------------------------
// Shared memory constants
// ---------------------------------------------------------------------------
#define SHM_NAME        L"Global\\HotkeyInspectorSHM"
#define SHM_SIZE        (64 * 1024)
#define RECORD_SIZE     32
#define MAX_RECORDS     (SHM_SIZE / RECORD_SIZE)

#pragma pack(push, 1)
typedef struct {
    DWORD processId;
    DWORD threadId;
    int   hotkeyId;
    DWORD fsModifiers;
    DWORD vk;
    DWORD timestamp;
    CHAR  action;       // 'R' = Register
    CHAR  padding[7];
} HotkeyRecord;
#pragma pack(pop)

typedef BOOL (WINAPI *RegisterHotKeyFn)(HWND, int, UINT, UINT);

// ---------------------------------------------------------------------------
// Shared globals (in .SHARED section for cross-process visibility)
// ---------------------------------------------------------------------------
#pragma data_seg(".SHARED")
static HHOOK       g_cbtHook   = NULL;
static HINSTANCE   g_hInst     = NULL;
static volatile LONG g_initialized = 0;
#pragma data_seg()
#pragma comment(linker, "/SECTION:.SHARED,RWS")

// ---------------------------------------------------------------------------
// Module-local globals
// ---------------------------------------------------------------------------
static HANDLE          g_shmFile   = NULL;
static HotkeyRecord*   g_shmBuffer = NULL;
static volatile LONG   g_writeIdx  = 0;
static CRITICAL_SECTION g_cs;
static RegisterHotKeyFn g_originalRegisterHotKey = NULL;

// ---------------------------------------------------------------------------
// Shared memory helpers
// ---------------------------------------------------------------------------
static BOOL InitSharedMemory(void) {
    g_shmFile = CreateFileMappingW(
        INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, SHM_SIZE, SHM_NAME);

    if (!g_shmFile && GetLastError() == ERROR_ALREADY_EXISTS)
        g_shmFile = OpenFileMappingW(FILE_MAP_ALL_ACCESS, FALSE, SHM_NAME);
    if (!g_shmFile) return FALSE;

    g_shmBuffer = (HotkeyRecord*)MapViewOfFile(
        g_shmFile, FILE_MAP_ALL_ACCESS, 0, 0, SHM_SIZE);
    return (g_shmBuffer != NULL);
}

static void WriteRecord(DWORD processId, DWORD threadId, int hotkeyId,
                        DWORD fsModifiers, DWORD vk, CHAR action)
{
    if (!g_shmBuffer) return;
    EnterCriticalSection(&g_cs);
    DWORD idx = (DWORD)InterlockedIncrement(&g_writeIdx) % MAX_RECORDS;
    g_shmBuffer[idx].processId   = processId;
    g_shmBuffer[idx].threadId    = threadId;
    g_shmBuffer[idx].hotkeyId    = hotkeyId;
    g_shmBuffer[idx].fsModifiers = fsModifiers;
    g_shmBuffer[idx].vk          = vk;
    g_shmBuffer[idx].timestamp   = GetTickCount() / 1000;
    g_shmBuffer[idx].action      = action;
    LeaveCriticalSection(&g_cs);
}

// ---------------------------------------------------------------------------
// Hooked RegisterHotKey (forward declaration needed for IAT patching)
// ---------------------------------------------------------------------------
static BOOL WINAPI HookRegisterHotKey(HWND hWnd, int id,
                                      UINT fsModifiers, UINT vk)
{
    WriteRecord(GetCurrentProcessId(), GetCurrentThreadId(),
                id, fsModifiers, vk, 'R');

    if (g_originalRegisterHotKey)
        return g_originalRegisterHotKey(hWnd, id, fsModifiers, vk);

    HMODULE hUser32 = GetModuleHandleW(L"user32.dll");
    if (hUser32) {
        RegisterHotKeyFn realFn = (RegisterHotKeyFn)
            GetProcAddress(hUser32, "RegisterHotKey");
        if (realFn) return realFn(hWnd, id, fsModifiers, vk);
    }
    return FALSE;
}

// ---------------------------------------------------------------------------
// IAT patching
// ---------------------------------------------------------------------------
static void PatchIAT_RegisterHotKey(void) {
    HMODULE hMod = GetModuleHandleW(NULL);
    if (!hMod) return;

    PIMAGE_DOS_HEADER dos = (PIMAGE_DOS_HEADER)hMod;
    PIMAGE_NT_HEADERS nt  = (PIMAGE_NT_HEADERS)((BYTE*)hMod + dos->e_lfanew);

    IMAGE_DATA_DIRECTORY importDir = nt->OptionalHeader
        .DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT];
    if (!importDir.Size) return;

    PIMAGE_IMPORT_DESCRIPTOR desc = (PIMAGE_IMPORT_DESCRIPTOR)
        ((BYTE*)hMod + importDir.VirtualAddress);

    for (; desc->Name; desc++) {
        PCHAR modName = (PCHAR)((BYTE*)hMod + desc->Name);
        if (_stricmp(modName, "user32.dll") != 0) continue;

        PULONG_PTR thunk = (PULONG_PTR)((BYTE*)hMod + desc->FirstThunk);
        PULONG_PTR origThunk = desc->OriginalFirstThunk
            ? (PULONG_PTR)((BYTE*)hMod + desc->OriginalFirstThunk)
            : thunk;

        for (; *origThunk; thunk++, origThunk++) {
            if (*origThunk & IMAGE_ORDINAL_FLAG) continue;

            PIMAGE_IMPORT_BY_NAME import = (PIMAGE_IMPORT_BY_NAME)
                ((BYTE*)hMod + (*origThunk & 0xFFFFFFFF));

            if (strcmp(import->Name, "RegisterHotKey") == 0) {
                if (!g_originalRegisterHotKey)
                    g_originalRegisterHotKey = (RegisterHotKeyFn)(*thunk);

                DWORD oldProtect;
                VirtualProtect(thunk, sizeof(ULONG_PTR),
                               PAGE_READWRITE, &oldProtect);
                *thunk = (ULONG_PTR)HookRegisterHotKey;
                VirtualProtect(thunk, sizeof(ULONG_PTR),
                               oldProtect, &oldProtect);
            }
        }
    }
}

// ---------------------------------------------------------------------------
// Worker: initialize DLL in the current process
// Called once from InstallHook (main process) or from CbtHookProc (injected)
// ---------------------------------------------------------------------------
static void DLL_Init(void) {
    if (InterlockedCompareExchange(&g_initialized, 1, 0) != 0)
        return;
    InitializeCriticalSection(&g_cs);
    InitSharedMemory();
    PatchIAT_RegisterHotKey();
}

// ---------------------------------------------------------------------------
// CBT hook procedure
// ---------------------------------------------------------------------------
static LRESULT CALLBACK CbtHookProc(int nCode, WPARAM wParam, LPARAM lParam) {
    // Deferred init on first call (avoids DllMain work)
    DLL_Init();

    if (nCode == HCBT_CREATEWND) {
        // A new window was created — re-patch IAT (module may be new)
        PatchIAT_RegisterHotKey();
    }
    return CallNextHookEx(g_cbtHook, nCode, wParam, lParam);
}

// ---------------------------------------------------------------------------
// Exported: install/uninstall the global CBT hook
// ---------------------------------------------------------------------------
extern "C" __declspec(dllexport) BOOL InstallHook(void) {
    if (g_cbtHook) return TRUE;
    DLL_Init();
    g_cbtHook = SetWindowsHookExW(WH_CBT, CbtHookProc, g_hInst, 0);
    return (g_cbtHook != NULL);
}

extern "C" __declspec(dllexport) BOOL UninstallHook(void) {
    if (g_cbtHook) {
        UnhookWindowsHookEx(g_cbtHook);
        g_cbtHook = NULL;
        return TRUE;
    }
    return FALSE;
}

// ---------------------------------------------------------------------------
// DllMain — minimal work to avoid loader lock
// ---------------------------------------------------------------------------
BOOL APIENTRY DllMain(HMODULE hModule, DWORD reason, LPVOID reserved) {
    if (reason == DLL_PROCESS_ATTACH) {
        g_hInst = hModule;
        DisableThreadLibraryCalls(hModule);
        // Init is deferred to first CBT hook call or to InstallHook()
    }
    else if (reason == DLL_PROCESS_DETACH && reserved == NULL) {
        if (g_shmBuffer) { UnmapViewOfFile(g_shmBuffer); g_shmBuffer = NULL; }
        if (g_shmFile)   { CloseHandle(g_shmFile);       g_shmFile   = NULL; }
        DeleteCriticalSection(&g_cs);
    }
    return TRUE;
}
