namespace WinFence.Core;

/// <summary>
/// 纯逻辑示例 — 验证 Core 类库被 App 正确引用。
/// 后续会迁出:布局模型、规则引擎、持久化模型等无 UI 依赖的逻辑。
/// </summary>
public static class Greeter
{
    public static string Greet() => $"hello from WinFence.Core @ {DateTime.UtcNow:yyyy-MM-dd}";
}
