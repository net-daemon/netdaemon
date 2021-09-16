using System.Threading.Tasks;

namespace NetDaemon.Common
{
    /// <summary>
    ///     Interface for applications that support Saving nd restoring their state
    /// </summary>
    public interface IPersistenceService
    {
        /// <summary>
        ///     Saves the app state
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Saves the state of the storage object.!--
        /// </para>
        /// <para>    It is implemented async so state will be lazy saved</para>
        /// </remarks>
        void SaveAppState();
        
        /// <summary>
        ///     Restores the app state
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Restores the state of the storage object.!--
        /// </para>
        /// <para>    It is implemented async so state will be lazy saved</para>
        /// </remarks>
        Task RestoreAppStateAsync();

        dynamic Storage { get; }
    }
}