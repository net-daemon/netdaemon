using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common.Reactive;
using Helto4real.Powertools;
public static class DaemonAppExtensions
{

    /// <summary>
    ///     Takes a snapshot of given entity id of camera and sends to private discord server
    /// </summary>
    /// <param name="app">NetDaemonApp to extend</param>
    /// <param name="camera">Unique id of the camera</param>
    public static void CameraTakeSnapshotAndNotify(this NetDaemonRxApp app, string camera)
    {
        var imagePath = app.CameraSnapshot(camera);

        app.NotifyImage(camera, imagePath);
    }

    public static void Notify(this NetDaemonRxApp app, string message)
    {
        app.CallService("notify", "hass_discord", new
        {
            message = message,
            target = "511278310584746008"
        });
    }

    public static void NotifyImage(this NetDaemonRxApp app, string message, string imagePath)
    {
        var dict = new Dictionary<string, IEnumerable<string>>
        {
            ["images"] = new List<string> { imagePath }
        };

        app.CallService("notify", "hass_discord", new
        {
            data = dict,
            message = message,
            target = "511278310584746008"
        });
    }
}