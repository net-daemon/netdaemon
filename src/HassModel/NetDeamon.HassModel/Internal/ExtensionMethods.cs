namespace NetDaemon.HassModel.Internal;

/// <summary>
///     Useful extension methods used
/// </summary>
internal static class NetDaemonExtensions
{
    public static (string? Left, string Right) SplitAtDot(this string id)
    {
        var firstDot = id.IndexOf('.', System.StringComparison.InvariantCulture);
        if (firstDot == -1) return (null, id);
            
        return (id[.. firstDot ], id[ (firstDot + 1) .. ]);
    }
}