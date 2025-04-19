namespace NetDaemon.Runtime;

/// <summary>
/// Interface that can be used to check if NetDaemon runtime is initialized.
/// </summary>
public interface INetDaemonRuntimeInitializedCheck
{
    /// <summary>
    /// Method that can be awaited to ensure that the runtime is fully started and initialized (initial connection is created and cache is initialized).
    /// </summary>
    Task EnsureInitializedAsync();
}
