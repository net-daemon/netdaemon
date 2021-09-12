using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;
using NetDaemon.Common.Exceptions;

namespace NetDaemon.Daemon.Services
{
    internal class ApplicationPersistenceService : INetDaemonPersistantApp
    {
        private readonly IApplicationMetadata _applicationMetadata;
        private readonly ILogger _logger;
        private readonly INetDaemon _daemon;

        private Channel<bool> InternalLazyStoreStateQueue { get; } = Channel.CreateBounded<bool>(1);
        private readonly FluentExpandoObject _internalStorageObject;

        public ApplicationPersistenceService(IApplicationMetadata applicationMetadata, INetDaemon netDaemon)
        { 
            _applicationMetadata = applicationMetadata;
            _daemon = netDaemon;
            _logger = netDaemon.Logger;
            _internalStorageObject = new FluentExpandoObject(false, true, persistCallback: SaveAppState);
        }

        public dynamic Storage => _internalStorageObject;

        public Task StartUpAsync(CancellationToken cancellationToken)
        {
            return HandleLazyStorage(cancellationToken);
        }

        private string GetUniqueIdForStorage() => $"{_applicationMetadata.AppType.Name}_{_applicationMetadata.Id}".ToLowerInvariant();

        public async Task RestoreAppStateAsync()
        {
            var obj = await _daemon.GetDataAsync<IDictionary<string, object?>>(GetUniqueIdForStorage())
                .ConfigureAwait(false);

            if (obj != null)
            {
                var expStore = (FluentExpandoObject) Storage;
                expStore.CopyFrom(obj);
            }

            bool isDisabled = Storage.__IsDisabled ?? false;
            var appInfo = _daemon.State.FirstOrDefault(s => s.EntityId == _applicationMetadata.EntityId);
            var appState = appInfo?.State as string;
            if (isDisabled)
            {
                _applicationMetadata.IsEnabled = false;
                if (appState == "on" || appInfo is null)
                {
                    dynamic serviceData = new FluentExpandoObject();
                    serviceData.entity_id = _applicationMetadata. EntityId;
                    await _daemon.SetStateAsync(_applicationMetadata.EntityId, "off").ConfigureAwait(false);
                }
            }
            else
            {
                _applicationMetadata.IsEnabled = true;
                if (appState == "off" || appInfo is null)
                {
                    dynamic serviceData = new FluentExpandoObject();
                    serviceData.entity_id = _applicationMetadata.EntityId;
                    await _daemon.SetStateAsync(_applicationMetadata.EntityId, "on").ConfigureAwait(false);
                }
            }
        }
        
        public void SaveAppState()
        {
            // Intentionally ignores full queue since we know
            // a state change already is in progress which means
            // this state will be saved
            _ = InternalLazyStoreStateQueue.Writer.TryWrite(true);
        }

        [SuppressMessage("Microsoft.Design", "CA1031")]
        private async Task HandleLazyStorage(CancellationToken cancellationToken)
        {
            _ = _internalStorageObject ??
                throw new NetDaemonNullReferenceException($"{nameof(_internalStorageObject)} cant be null!");
            _ = _daemon ?? throw new NetDaemonNullReferenceException($"{nameof(_daemon)} cant be null!");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Dont care about the result, just that it is time to store state
                    _ = await InternalLazyStoreStateQueue.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);

                    await _daemon.SaveDataAsync(GetUniqueIdForStorage(), (IDictionary<string, object>) Storage)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    _logger.LogError("Error in storage queue {e}", e);
                } // Ignore errors in thread
            }
        }
    }
}