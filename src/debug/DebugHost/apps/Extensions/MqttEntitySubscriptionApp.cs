﻿using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.HassModel;

namespace DebugHost.apps.Extensions;

[NetDaemonApp]
public class MqttEntitySubscriptionApp : IAsyncInitializable
{
    private readonly IHaContext _ha;
    private readonly ILogger<MqttEntitySubscriptionApp> _logger;
    private readonly IMqttEntityManager _entityManager;

    public MqttEntitySubscriptionApp(IHaContext ha, ILogger<MqttEntitySubscriptionApp> logger,
        IMqttEntityManager entityManager)
    {
        _ha = ha;
        _logger = logger;
        _entityManager = entityManager;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await Task.Run(() => ExercisorAsync(cancellationToken), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Important: All this method does is set up the subscriptions that will log when the test switches are toggled
    /// To verify this is running:
    ///     1. Run the code
    ///     2. Check your HomeAssistant instance and you should see two switches named "One" and "Two"
    ///     3. Toggle them and ensure that the a message is logged (see code within PrepareCommandSubscriptionAsync...)
    ///     4. (optionally) uncomment the code at the end to remove the test switches, rebuild and re-run
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async Task ExercisorAsync(CancellationToken cancellationToken)
    {
        const string switch1Id = "switch.switch_one";
        const string switch2Id = "switch.switch_two";
        const string onCommand = "ON";
        const string offCommand = "OFF";

        await _entityManager.CreateAsync(switch1Id,
                new EntityCreationOptions(Name: "Switch One", PayloadOn: onCommand, PayloadOff: offCommand))
            .ConfigureAwait(false);

        await _entityManager.CreateAsync(switch2Id,
                new EntityCreationOptions(Name: "Switch Two", PayloadOn: onCommand, PayloadOff: offCommand))
            .ConfigureAwait(false);

        (await _entityManager.PrepareCommandSubscriptionAsync(switch1Id).ConfigureAwait(false)).Subscribe(new Action<string>(async s =>
        {
            _logger.LogInformation("Subscription #1a got command for {Switch} {Cmd}", switch1Id, s);
            await Task.Yield();     // Achieves nothing, just masks the CS1998 warning
        }));

        (await _entityManager.PrepareCommandSubscriptionAsync(switch1Id).ConfigureAwait(false)).Subscribe(new Action<string>(async s =>
        {
            _logger.LogInformation("Subscription #1b got command for {Switch} {Cmd}", switch1Id, s);
            await Task.Yield();     // Achieves nothing, just masks the CS1998 warning
        }));

        (await _entityManager.PrepareCommandSubscriptionAsync(switch2Id).ConfigureAwait(false)).Subscribe(new Action<string>(async s =>
        {
            _logger.LogInformation("Subscription #2 got command for {Switch} {Cmd}", switch2Id, s);
            await Task.Yield();     // Achieves nothing, just masks the CS1998 warning
        }));

        // Thread.Sleep(2000);
       // await _entityManager.RemoveAsync(switch1Id).ConfigureAwait(false);
       // await _entityManager.RemoveAsync(switch2Id).ConfigureAwait(false);
    }
}
