using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NetDaemon.Common
{
    internal class RuntimeInfoManager : IAsyncDisposable
    {
        private readonly INetDaemon _daemon;
        private readonly IApplicationMetadata _applicationMetadata;
        private readonly Channel<bool> _updateRuntimeInfoChannel = Channel.CreateBounded<bool>(5);
        private CancellationTokenSource _cancellationTokenSource = new();
        private readonly Task _ProcessTask;

        public RuntimeInfoManager(INetDaemon daemon, IApplicationMetadata applicationMetadata)
        {
            _daemon = daemon;
            _applicationMetadata = applicationMetadata;
            _ProcessTask = ManageRuntimeInformationUpdates(_cancellationTokenSource.Token);
        }

        internal void UpdateRuntimeInformation()
        {
            // We just ignore if channel is full, it will be ok
            _updateRuntimeInfoChannel.Writer.TryWrite(true);
        }
        
        private async Task ManageRuntimeInformationUpdates(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    while (_updateRuntimeInfoChannel.Reader.TryRead(out _)) ;
                    _ = await _updateRuntimeInfoChannel.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                    // do the deed
                    await HandleUpdateRuntimeInformation().ConfigureAwait(false);
                    // make sure we never push more messages that 10 per second
                    await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Just exit
                    break;
                }
            }
        }

        private async Task HandleUpdateRuntimeInformation()
        {
            if (_applicationMetadata.RuntimeInfo.LastErrorMessage is not null)
            {
                _applicationMetadata.RuntimeInfo.HasError = true;
            }

            if (_daemon!.IsConnected)
            {
                await _daemon.SetStateAsync(_applicationMetadata.EntityId, _applicationMetadata.IsEnabled ? "on" : "off", ("runtime_info", _applicationMetadata.RuntimeInfo))
                    .ConfigureAwait(false);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await _ProcessTask.ConfigureAwait(false);
            _cancellationTokenSource.Dispose();
        }
    }
}