using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.HassModel;

namespace DebugHost.apps.Extensions;

[NetDaemonApp]
[Focus]
public class MttEntitySubscriptionApp : IAsyncInitializable
{
    private readonly IHaContext _ha;
    private readonly ILogger<MttEntitySubscriptionApp> _logger;
    private readonly IMqttEntityManager _entityManager;

    public MttEntitySubscriptionApp(IHaContext ha, ILogger<MttEntitySubscriptionApp> logger,
        IMqttEntityManager entityManager)
    {
        _ha = ha;
        _logger = logger;
        _entityManager = entityManager;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        Task.Run(() => ExercisorAsync(cancellationToken), cancellationToken);
    }

    private async Task ExercisorAsync(CancellationToken cancellationToken)
    {
        var switch1Id = "switch.switch_one";
        var switch2Id = "switch.switch_two";
        var onCommand = "ON";
        var offCommand = "OFF";

        await _entityManager.CreateAsync(switch1Id,
                new EntityCreationOptions(Name: "Switch One", PayloadOn: onCommand, PayloadOff: offCommand))
            .ConfigureAwait(false);
        
        await _entityManager.CreateAsync(switch2Id,
                new EntityCreationOptions(Name: "Switch Two", PayloadOn: onCommand, PayloadOff: offCommand))
            .ConfigureAwait(false);
        
        
        (await _entityManager.SubscribeEntityCommandAsync(switch1Id).ConfigureAwait(false)).Subscribe(new Action<string>(async s =>
        {
            _logger.LogInformation("Subscription #1 got command for {switch} {cmd}", switch1Id, s);
        }));
        
        (await _entityManager.SubscribeEntityCommandAsync(switch2Id).ConfigureAwait(false)).Subscribe(new Action<string>(async s =>
        {
            _logger.LogInformation("Subscription #2 got command for {switch} {cmd}", switch2Id, s);
        }));
        
        // Thread.Sleep(2000);
       // await _entityManager.RemoveAsync(switch1Id).ConfigureAwait(false);
       // await _entityManager.RemoveAsync(switch2Id).ConfigureAwait(false);
    }
}