using System.Diagnostics;

namespace HotkeyInspector.App;

internal static class Links
{
    public const string DotnetDownload = "https://dotnet.microsoft.com/download/dotnet/10.0";

    public static void Open(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
