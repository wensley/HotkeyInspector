# Hotkey Inspector 代码审计报告

- 审计日期：2026-10-20
- 范围：`src/` 全部生产代码（4 个项目、26 个 C#/XAML 文件，约 2973 行）、`native/HookDll`、构建与发布配置、README/architecture 文档
- 基线：`main` @ `3771e55`，Release 构建通过（0 警告 / 0 错误，15.4s）
- 方法：静态阅读 + 真实运行探针（`HotkeyAvailabilityService` / `KnownHotkeyCatalog` / `HotkeyParser`）+ 源码键位统计

## 一、结论摘要

工程骨架清晰、构建干净、安全模式（`RegisterHotKey` 探测后立即注销）思路正确，双语与全屏锁定捕获的交互闭环完整。但当前版本存在 1 个"扫描结果整体不可信"的展示级缺陷、1 个"目录 26% 条目被静默覆盖"的数据缺陷，以及解析器/捕获器/知识库三套键位词表互不一致导致的大量不可达条目。另有无测试、无 CI、HookBridge/native 死代码、文档严重漂移等工程问题。

| 维度 | 评级 | 说明 |
|---|---|---|
| 正确性 | C | 可用/不可用文案自相矛盾；目录冲突后写覆盖造成误归因 |
| 稳定性 | C+ | 进程句柄泄漏、`async void`、未防护的剪贴板/文件 IO、id 越界 |
| 架构 | C+ | `Core → Infrastructure` 分层倒置；HookBridge/native 死代码 |
| 安全 | B | 无网络、无密钥、无注入默认路径；全局钩子吞键需明示风险 |
| 性能 | B | 数据量小；但每次刷新枚举全部进程且只为未使用字段做 P/Invoke |
| 可维护性 | C | 544 行窗口类、三套词表、双份状态、零测试 |
| 文档/发布 | D+ | 800+ 条（实际 271）、不存在的类型、未接入的 HookDll；版本号 1.0.0.0 vs 标签 v0.0.1 |

综合建议：**先修 P0（约半天），再补测试与统一词表（1–2 天），最后决定 HookBridge 去留**。

## 二、P0：用户可见的正确性缺陷

### P0-1 可用快捷键显示为"可用 / 未知应用 / 该快捷键不可用。"

`src/HotkeyInspector.Core/HotkeyCheckResult.cs:56-63`

```csharp
detail = ErrorCode switch
{
    NativeMethods.ErrorHotkeyAlreadyRegistered => detail,
    0 => T(language, "该快捷键不可用。", "This shortcut is unavailable."),  // 覆盖了正常文案
    _ => detail + ...
};
```

成功注册时 `ErrorCode = 0`，因此 `detail` 被替换成"该快捷键不可用。"。同时 `:32` 的判断排除了 `KnownHotkeyCatalog.Available`，导致 owner 落到 `else` 分支的"未知应用"。

实测（探针，Release）：

```
[Ctrl+Shift+F10] available=True err=0
    status=可用 owner=未知应用 detail=该快捷键不可用。
```

影响放大：`扫描常见组合` 一次生成 288 行（6 组修饰键 × 48 个键，`CommonHotkeyGenerator.cs:7-28`），其中所有可用项都是这种自相矛盾的三段式，看起来像功能失效。

修复：

```csharp
if (MatchedProcess is not null) { ... }
else if (IsAvailable)
{
    owner = Known?.OwnerApplication.Get(language) ?? T(language, "无", "None");
    detail = Known?.Usage.Get(language) ?? T(language, "该快捷键当前可注册。", "This shortcut can be registered.");
}
else if (Known is not null && Known != KnownHotkeyCatalog.UnknownOccupied) { ... }
else { ... }

detail = ErrorCode switch
{
    0 or NativeMethods.ErrorHotkeyAlreadyRegistered => detail,
    _ => detail + ...
};
```

### P0-2 目录键冲突静默覆盖，造成误归因

`src/HotkeyInspector.Core/KnownHotkeyCatalog.cs:52`（以及各 `Add*` 内的同一写法）

```csharp
void a(...) => d[k] = new(...);   // 后写覆盖前写，无告警
```

统计：324 条字面条目 + 44 条循环生成 = 368 次写入，最终只剩 **271** 个键，**97 条（26%）被静默丢弃**；**59 个键有 2–7 个归属**。

实测归因（错误项已加粗）：

| 组合 | 实际归属 | 程序输出 |
|---|---|---|
| `Ctrl+Shift+Esc` | 任务管理器 | **Microsoft Teams（打开活动日志）** |
| `F1` | 系统/应用帮助 | **Snipaste** |
| `F3` | 资源管理器搜索 | **Snipaste** |
| `Ctrl+T` | 浏览器新标签页 | **Slack** |
| `Alt+Space` | PowerToys Run | **通用启动器** |
| `Ctrl+Shift+F` | 浏览器/VS Code/Everything | **Steam** |

最严重的 `Ctrl+Shift+Esc` 会让用户以为任务管理器快捷键被 Teams 占用。修复方向：`Dictionary<string, List<KnownHotkeyInfo>>`（与 `HotkeyKnowledge` 的多命中设计一致），为条目增加类别/优先级，展示时按优先级排序；在 `BuildCatalog` 末尾加冲突断言并在 CI 中固化。

### P0-3 `Lookup` 未归一化，与解析器/知识库不一致

`KnownHotkeyCatalog.cs:21-26` 直接用 `hotkey.DisplayText` 精确匹配；而 `HotkeyParser` 的 `DisplayText`：

- 修饰键顺序 = 用户输入顺序（`HotkeyParser.cs:76`）→ 输入 `Shift+Ctrl+T` → `Shift+Ctrl+T`，查不到 `Ctrl+Shift+T`；
- `PAGEUP`、`PRINTSCREEN`、`BACKSPACE` 走默认标签 `token[..1] + rest.ToLower()`（`:144`）→ `Pageup` / `Printscreen`，与目录的 `PageUp` / `PrintScreen` 不匹配（实测 `PAGEUP => display='Pageup'`）。

相比之下 `HotkeyKnowledge.FindMatches` 使用 `ComboKey.Normalize`（`HotkeyKnowledge.cs:73-74`）是正确的。修复：目录索引与查询统一走 `ComboKey.Normalize`，并用同一函数生成 `DisplayText`。

### P0-4 解析器词表 < 目录/知识库/捕获器，大量条目永不可达

`HotkeyParser.BuildKeys()`（`HotkeyParser.cs:80-129`）只支持修饰键、字母、数字、F1–F24、少量命名键和 `.`。

- 324 条目录字面键中 **22 条无法解析**：`Ctrl+Shift+]`、`Ctrl+Shift+[`、`Ctrl+/`、`Ctrl+,`、`Ctrl+\`、`` Ctrl+` ``、`Win+Comma`、`Win+Semicolon`、`Alt+Shift`（仅修饰键）、`Alt+Shift+-`，以及 8 条 VS 和弦（`Ctrl+K, Ctrl+D` 等）。
- 知识库 3056 条中 **734 条含不支持 token**（`]` 32、`[` 31、`/` 20、`,` 17、`;` 16、`-` 16、`Plus` 15、`=` 13、`Minus` 11、`\` 10 …），另有 13 条和弦、22 条仅修饰键、1 条空键。
- 捕获器 `VirtualKeys`（`VirtualKeys.cs:48-63`）会产出 `ContextMenu`、`NumPad0`、`Multiply`、`Plus`、`Minus`、`Divide`、`Semicolon`、`Comma`、`Slash`、`BracketLeft/Right`、`Backslash`、`Quote` —— 解析器**全部不认**。

后果：捕获 `Ctrl+Shift+]` 或按小键盘键时，`HotkeyParser.Parse` 抛异常 → `CheckAndAdd` 落到"无效"分支，而不是正常检测。

修复：建立**单一键位词表**（token → VK → 规范标签），供 `HotkeyParser`、`VirtualKeys`、`ComboKey` 共用；和弦单独建模（如 `HotkeyDefinition.Sequence`），不要塞进 `+` 分隔的字符串。

### P0-5 减号键被全局替换成 `+`

`HotkeyParser.cs:22`

```csharp
.Replace("-", "+", StringComparison.Ordinal)
```

实测 `Ctrl+-` → `HotkeyParseException: 请添加一个非修饰键`。同时目录中的 `Alt+Shift+-` 也永远不可达。修复：仅在"分隔符位置"把 `-` 视为分隔符，或先按 token 解析再决定。

## 三、P1：稳定性与资源

### P1-1 进程对象未释放（句柄泄漏）

`ProcessHotkeyMatcher.cs:49-72`：`Process.GetProcesses()` 返回的 `Process` 均可持有本机句柄，从未 `Dispose()`；每 5 秒刷新一次，长期运行会持续泄漏。同时 `WindowTitle` / `WindowClass` / `ProcessId` 无人消费，`GetWindowClass` 的 P/Invoke 是纯浪费。异常被空 `catch` 吞掉（`:69-76`），无法诊断。

修复：`foreach (var p in Process.GetProcesses()) { try { ... } finally { p.Dispose(); } }`，或改用 `EnumWindows`；删除未使用字段；记录失败计数。

### P1-2 静态缓存非线程安全，且从不失效

`ProcessHotkeyMatcher.cs:8-9,78-80`：`_cachedProcesses` / `_lastCacheTime` 无锁；`InvalidateCache()` 全仓库无调用点（已 grep）。未来若从后台线程调用会读到半初始化列表。

### P1-3 `async void` 扫描无异常保护

`MainWindow.xaml.cs:119-148`：`ScanButton_Click` 为 `async void`，无 `try/finally`。任一异常 → 进程崩溃，且 `_isScanning` 永久为 `true`，后续点击只提示"正在扫描"。`CheckAndAdd` 只捕获 `HotkeyParseException`，`Lazy` 初始化失败等会直接穿出。

修复：`try { ... } catch (Exception ex) { SetStatus(...); } finally { _isScanning = false; }`，至少加全局 `DispatcherUnhandledException`。

### P1-4 剪贴板 / 导出 / 对话框未防护

`MainWindow.xaml.cs:158`（`Clipboard.SetText`）与 `:182`（`File.WriteAllText`）在剪贴板被其他程序占用、目标只读/被占用时抛未捕获异常。另：CSV 为 UTF-8 **无 BOM**，Excel 直接双击打开中文会乱码。

修复：`try/catch` + 状态栏提示；写文件使用 `new UTF8Encoding(true)`。

### P1-5 热键 id 无上限、非线程安全

`HotkeyAvailabilityService.cs:8,18`：`_nextId` 从 `0x4811` 递增，`RegisterHotKey` 合法 id 上界为 `0xBFFF`（30702 次后越界）；越界后返回错误 87，被当作"已占用"并附 Win32 文案。`_nextId++` 在多线程下会重号。

修复：在 `0x0001..0xBFFF` 内循环复用，`Interlocked.Increment` 并回绕。

### P1-6 知识库加载无降级，可能阻塞低级钩子

`HotkeyKnowledge.cs:68-115`：JSON 缺失/损坏时 `Lazy` 抛 `TypeInitializationException`，全应用知识库功能失效且无提示。首次访问发生在 `OnComboCaptured`（`MainWindow.xaml.cs:347`）时，解析 3056 条 JSON 会在**低级键盘钩子回调线程**上执行；超过 `LowLevelHooksTimeout`（默认约 300ms）会被系统静默摘钩，UI 仍显示"锁定中"但已失效。

修复：启动时预热 `HotkeyKnowledge.Base`；钩子回调内只做入队，绝不做查询/分配；加载失败降级为空库并记录。

### P1-7 钩子回调无异常隔离，无重复键过滤

`KeyboardHook.cs:69-127`：`ComboCaptured?.Invoke` 若抛异常会穿出本机回调，.NET 默认终止进程。按住键不放时 `WM_KEYDOWN` 重复触发，会反复上报同一组合。另需在文档中说明：**焦点位于管理员权限窗口时，低级钩子收不到按键（UIPI）**。

修复：回调整体 `try/catch`；用 `KBDLLHOOKSTRUCT` + 键状态/时间戳去重；UI 提示 UIPI 限制。

## 四、P2：架构与可维护性

### P2-1 分层倒置

`Core.csproj:4` 引用 `Infrastructure`；`HotkeyCheckResult.cs:1,13` 直接暴露 `ProcessInfo`；`HotkeyAvailabilityService.cs:2,22-44` 直接调用 `NativeMethods` 与 `ProcessHotkeyMatcher`。领域层无法脱离 Win32 单测。

建议：Core 定义 `IHotkeyProbe`（TryRegister）与 `IRunningProcessSource`，Infrastructure 实现；`HotkeyCheckResult` 改用 Core 自己的 `ProcessCandidate` 记录。

### P2-2 HookBridge / HookDll 是完整的死代码

- `HotkeyHookService` 全仓库无实例化（grep 仅自身）；App 引用该程序集但从未调用。
- `native/HookDll/*.vcxproj` 不在 `HotkeyInspector.slnx` 中（solution 只有空的 `/native/` 文件夹），因此从未参与构建；托管侧也没有任何 `LoadLibrary/InstallHook` P/Invoke。
- 即便接通，设计也不可用：IAT patch 只覆盖主模块；`GetProcAddress("RegisterHotKey")` 可绕过；`CRITICAL_SECTION` 是进程内锁，挡不住跨进程撕裂写；无 seqlock/版本头；记录永不清空（读端把 `ProcessId != 0` 当有效，历史记录永久残留）；以 `Global\` + `AllAccess` 打开共享内存；读取 `processId == 0` 判空但写入索引从 1 开始。

建议：**删除 HookBridge 与 native/HookDll，或明确标注 experimental 并从 README/architecture 移除**，避免"可选注入"的虚假承诺与降低杀软误报面。

### P2-3 三套键位词表 + 双份状态 + 上帝类

- `VirtualKeys` / `HotkeyParser.BuildKeys` / `ComboKey.Synonyms` 三套映射，已产生 P0-4 的不一致。
- `MainWindow.xaml.cs` 544 行同时承担 UI 事件、捕获状态机、检测、知识库展示、导出、语言切换；`_rows` 与 `Results` 保存同一份数据的两种形态。
- `ReservedHotkeys`（`MainWindow.xaml.cs:24-37`）与 `KnownHotkeyCatalog` 重复维护同类知识，已出现漂移。
- 托盘菜单硬编码中英文（`App.xaml.cs:78-79,101-103`），绕过 `Strings`；`SystemIcons.Application` 为通用图标。

建议：抽出 `HotkeyCheckViewModel` / `CaptureSession`；`ReservedHotkeys` 改为从目录按标记筛选；托盘文案走 `Strings`。

### P2-4 发布元数据缺失

`HotkeyInspector.App.csproj` 无 `ApplicationIcon`、`Version`、`Company`、`Product`、`app.manifest`。产物文件版本为 **1.0.0.0**，与 Release 标签 **v0.0.1** 不一致；无 PerMonitorV2 声明，高 DPI 下可能模糊；单文件发布无图标。

### P2-5 文档与实现漂移

| 文档表述 | 实际 |
|---|---|
| README/architecture：「800+ entry catalog」 | 271 个唯一键 |
| architecture：`ProcessWindowEnumerator` | 不存在 |
| architecture：「enumerates all top-level windows」 | `Process.GetProcesses()` + `MainWindowHandle` |
| README：「Optional native HookDll for live interception」 | 未参与构建、未接装、不可用 |
| architecture：「10 categories」 | 10 个 `Add*`，但覆盖后仅剩 271 键 |

### P2-6 零测试、零 CI

无 `*.Tests` 项目，无 `.github/workflows`。`HotkeyParser`、`ComboKey`、`KnownHotkeyCatalog` 都是纯函数/静态数据，最适合用 xUnit 固化：解析边界、归一化等价类、目录冲突数、`Ctrl+Shift+Esc` 必须归任务管理器、可用项文案不得出现"不可用"。

## 五、P3：体验与可访问性

1. `Window MinWidth="1040"` 且无外层 `ScrollViewer`（`MainWindow.xaml:9`）：1366×768 @150% 缩放时逻辑宽度约 911px，界面会被裁切。
2. 对比度：`#8A94A6` on `#FFFFFF`（`CaptureNote`，`MainWindow.xaml:282`）约 3.1:1；`#B26A00` on `#F6F8FB` 约 3.6:1，均低于 WCAG AA 的 4.5:1。
3. 启动语言固定中文（`App/Localization.cs:14`），未跟随系统 UI 语言。
4. `Esc` 被用于退出锁定捕获，因此无法捕获单独 `Esc`，UI 未说明。
5. `_hook` 未 `Dispose`（窗口未实现 `IDisposable`），依赖 `Stop()` 在退出路径被调用；异常路径下可能残留钩子。

## 六、值得保留的优点

- **安全模式设计正确**：只做 `RegisterHotKey` + 立即 `UnregisterHotKey`，不注入、不常驻全局钩子。
- **知识库多命中模型**：`HotkeyKnowledge` 用 `Dictionary<string, List<KnowledgeMatch>>` + `ComboKey.Normalize`，是目录应效仿的正确形态。
- **全屏锁定捕获**：`WH_KEYBOARD_LL` + 吞键 + 修饰键实时预览 + `Esc` 退出，能拿到被系统抢占的组合（实测可用），交互闭环完整。
- **双语实现**：`LocalizedText` 带空值回退，`Strings` 集中管理，XAML 通过索引器绑定并可即时刷新；状态栏用 `Func<Language,string>` 保存重建器，语言切换可重放。
- **代码卫生**：`Nullable` 全开、Release 0 警告、命名一致、注释解释"为什么"（钩子限制、框架依赖版本说明）。
- **隐私与安全面小**：无网络请求、无遥测、无密钥；外部链接仅固定官方 URL。

## 七、建议的修复顺序

| 顺序 | 事项 | 预估 |
|---|---|---|
| 1 | P0-1 可用文案分支 + P0-2 目录改多值并加冲突测试 | 2–3 h |
| 2 | P0-3/P0-4/P0-5 统一词表、`Lookup` 归一化、修 `-` 与标签大小写 | 4–6 h |
| 3 | P1-1/2 进程枚举 `Dispose` + 去掉无用 P/Invoke；P1-3/4 异常防护与 CSV BOM | 2–3 h |
| 4 | P1-5 id 回绕；P1-6 知识库预热与降级；P1-7 钩子回调隔离/去重 | 3–4 h |
| 5 | 建 `HotkeyInspector.Tests`（xUnit）+ GitHub Actions（build/test） | 3–4 h |
| 6 | 决定 HookBridge/native 去留并同步 README/architecture；补图标/版本/DPI 清单 | 2 h |
| 7 | P2-1 抽接口解耦 Core/Infrastructure；P2-3 拆分窗口类 | 1–2 d |

## 八、附录：实测证据

> 以下均为**修复前**基线（Release，真实调用），保留用于对照。

构建：

```
dotnet build HotkeyInspector.slnx -c Release
已成功生成。 0 个警告 0 个错误
HotkeyInspector.dll  FileVersion=1.0.0.0  ProductVersion=1.0.0+3771e55
```

可用性/归因探针（Release，真实调用）：

```
[Ctrl+Shift+F10] available=True  err=0
    status=可用   owner=未知应用   detail=该快捷键不可用。
[Ctrl+Shift+Esc] available=False err=1409
    status=已占用 owner=Microsoft Teams detail=打开活动日志。
[F1] available=True owner=未知应用 detail=该快捷键不可用。

Ctrl+Shift+Esc => Microsoft Teams | 即时通讯
F1             => Snipaste        | 截图/录制
F3             => Snipaste        | 截图/录制
Ctrl+T         => Slack           | 即时通讯
Alt+Space      => 快速启动器（通用）| 启动器
Ctrl+Shift+F   => Steam           | 游戏

catalog entries = 271
knowledge entries = 3056
```

解析器探针：

```
'PAGEUP'     => display='Pageup'
'Alt+Ctrl+K' => display='Alt+Ctrl+K'
'Shift+Ctrl+T' => display='Shift+Ctrl+T'
'Ctrl+-'     => HotkeyParseException: 请添加一个非修饰键
```

统计脚本（源码级）：

```
目录写入 368 次（324 字面 + 44 生成）→ 唯一键 271 → 丢失 97（26.4%）
多归属键 59 个；最高 7 个归属（Ctrl+Shift+F）
目录字面键中 22 条无法被 HotkeyParser 解析
知识库 3056 条中 734 条含解析器不支持的 token，13 条和弦，22 条仅修饰键，1 条空
```

## 九、修复记录（2026-10-20）

### 状态总览

| 项 | 状态 | 落点 |
|---|---|---|
| P0-1 可用文案分支 | ✅ 已修 | `HotkeyCheckResult.Render`：`IsAvailable` 优先分支，owner=`Known.OwnerApplication`，detail=`Known.Usage` |
| P0-2 目录静默覆盖 | ✅ 已修 | `KnownHotkeyCatalog`：`Dictionary<string, List<...>>`，新增 `EntryCount`/`ConflictCount`/`LookupAll`，按类别优先级稳定排序 |
| P0-3 `Lookup` 未归一化 | ✅ 已修 | `Lookup` 统一走 `ComboKey.Normalize`（Ordinal 比较） |
| P0-4 词表不一致 | ✅ 已修 | 新增 `Core/KeyCatalog.cs`，`HotkeyParser`/`ComboKey`/`VirtualKeys` 共用 |
| P0-5 减号被替换 | ✅ 已修 | `HotkeyParser.SplitParts`：有 `+` 按 `+` 拆，无 `+` 有 `-` 按 `-` 拆 |
| P1-1/2 句柄与锁 | ✅ 已修 | `ProcessHotkeyMatcher`：`Dispose`、`CacheLock`、删除无用 P/Invoke |
| P1-3/4 异常与导出 | ✅ 已修 | `MainWindow`：`try/catch/finally`、剪贴板/导出防护、CSV 写 UTF-8 BOM、新增 3 条状态文案 |
| P1-5 热键 id | ✅ 已修 | `HotkeyAvailabilityService`：0x0001–0xBFFF 回绕、`Interlocked` |
| P1-6 知识库降级 | ✅ 已修 | `HotkeyKnowledge.Load` 失败降级为空库；`App.OnStartup` 预热 |
| P1-7 钩子健壮性 | ✅ 已修 | `KeyboardHook`：回调异常隔离、长按去重、状态复位 |
| P2-1 分层倒置 | ✅ 已修 | Core 新增 `IHotkeyProbe`/`IRunningProcessSource`/`ProcessCandidate`，Infrastructure 提供 `Win32HotkeyProbe`/`RunningProcessSource`；`Core.csproj` 不再引用 Infrastructure |
| P2-2 HookBridge/HookDll | ✅ 已删除 | `src/HotkeyInspector.HookBridge` 与 `native/HookDll` 的接入全部移除；slnx 不再包含该工程，native 仅留归档说明 |
| P2-3 上帝类/重复状态 | ✅ 主要项已拆 | 抽出 `HotkeyCaptureSession`、`HotkeyResultsModel`、`WpfKeyMapper`；`SystemReservedHotkeys` 移入 Core；托盘文案走 `Strings`；删除 `VirtualKeys` 包装层 |
| P2-4 发布元数据 | ✅ 已修 | `Version 0.0.1` + `Company`/`Product` + `app.manifest`（PerMonitorV2） |
| P2-5 文档漂移 | ✅ 已修 | README/architecture 改为实测数字与真实行为 |
| P2-6 测试与 CI | ✅ 已加 | `tests/HotkeyInspector.Tests`（52 个用例）、`.github/workflows/ci.yml` |
| P3-1 窄屏裁切 | ✅ 已修 | `MinWidth` 860 + 启动时按工作区收敛尺寸 + 按钮改 `WrapPanel` 折行 + 表格列宽收窄、`MinHeight` |
| P3-2 对比度 | ✅ 已修 | `#8A94A6` → `#5B6472`；`#B26A00` → `#8A5200` |
| P3-3 启动语言 | ✅ 已修 | 按系统 UI 语言选择中/英 |
| P3-4 单独 Esc | ✅ 已说明 | 捕获说明文案明确 Esc 用于退出、不可捕获 |
| P3-5 钩子释放 | ✅ 已修 | 窗口 `OnClosed` 释放 `HotkeyCaptureSession` |

### 修复后实测

构建与测试：

```
dotnet build HotkeyInspector.slnx -c Release   → 0 个警告 0 个错误
dotnet test  HotkeyInspector.slnx -c Release   → 52 passed, 0 failed
HotkeyInspector.exe  FileVersion=0.0.1.0  ProductVersion=0.0.1+<sha>
                     Company=zuhejian.com  Product=Hotkey Inspector
```

目录与归因（同一探针）：

```
EntryCount=368  UniqueKeys=271  ConflictCount=59

Ctrl+Shift+Esc => 2 owners | 任务管理器 / 其他工具
F1             => 2 owners | Windows/应用帮助
F3             => 2 owners | Windows 搜索
Ctrl+T         => 2 owners | 浏览器
Alt+Space      => 3 owners | PowerToys Run
Ctrl+Shift+F   => 7 owners | Discord
Shift+Ctrl+T   => 与 Ctrl+Shift+T 解析结果一致

可用项渲染：status=可用  owner=无  detail=该快捷键当前可注册为全局快捷键。
```

修复前 `可注册` 被误报为 `不可用`、`Ctrl+Shift+Esc` 归给 Teams、`F1/F3` 归给 Snipaste 等问题均已消失。

另外，进程匹配现在会遍历同一组合的**全部**归属：只有 `Teams.exe` 真的在运行时才把占用方提升为 Teams，否则仍显示优先级最高的任务管理器。单文件发布（`-r win-x64 --self-contained false`）已验证可产出可执行文件。

### 仍未处理

- `Ctrl+Shift+F` 的 7 个归属里，Discord（即时通讯）排第一并不符合直觉；这是**数据层**排序问题，不是代码缺陷，建议后续按「实际注册概率」给单条条目加权重。
- 知识库中 734 条含解析器不支持的 token（如 `Cmd`、和弦）。它们现在不会再被误当作 Windows 组合，但也无法在知识库匹配中命中，属可接受降级。
- 托盘图标仍是 `SystemIcons.Application`（仓库内无图标资源），单文件发布也无自定义 exe 图标。
