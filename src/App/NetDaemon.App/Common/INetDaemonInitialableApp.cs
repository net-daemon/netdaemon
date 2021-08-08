using System.Threading.Tasks;

namespace NetDaemon.Common
{
    /// <summary>
    ///     Apps that can be initialized
    /// </summary>
    public interface INetDaemonInitialableApp
    {
        /// <summary>
        /// Init the application sync, is called by the NetDaemon after startup
        /// </summary>
        void Initialize();

        /// <summary>
        /// Init the application async, is called by the NetDaemon after startup
        /// </summary>
        Task InitializeAsync();

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
        /// Start the application, normally implemented by the base class
        /// </summary>
        /// <param name="daemon"></param>
        Task StartUpAsync(INetDaemon daemon);
    }
}