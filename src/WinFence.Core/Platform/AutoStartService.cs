using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace WinFence.Core.Platform;

/// <summary>
/// 开机自启管理 — 通过 HKCU\Software\Microsoft\Windows\CurrentVersion\Run 写入/删除。
/// 不需要管理员权限(写 HKCU,不是 HKLM)。
/// 写入的值是当前进程 exe 的绝对路径。
/// </summary>
[SupportedOSPlatform("windows")]
public static class AutoStartService
{
    // 固定 AppId,跟应用绑定,便于以后读/删时定位
    public const string AppId = "WinFence";

    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        if (key == null) return false;
        var value = key.GetValue(AppId) as string;
        return !string.IsNullOrEmpty(value);
    }

    public static void Enable()
    {
        var exePath = GetCurrentExePath();
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true)
            ?? throw new InvalidOperationException("Cannot open Run registry key");
        key.SetValue(AppId, $"\"{exePath}\" --tray");
    }

    public static void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        if (key == null) return;
        if (key.GetValue(AppId) != null) key.DeleteValue(AppId, throwOnMissingValue: false);
    }

    private static string GetCurrentExePath()
    {
        // Process.MainModule.FileName 在 .NET 8 上标 obsolete 但仍能用;改用 Environment.ProcessPath
        return Environment.ProcessPath
            ?? Process.GetCurrentProcess().MainModule?.FileName
            ?? throw new InvalidOperationException("Cannot determine current exe path");
    }
}
