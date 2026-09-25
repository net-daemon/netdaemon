//#:package NetDaemon.HassModel@25.18.1

var ha = await NetDaemon.HassModel.HaContextFactory.CreateAsync("ws://lebigmac:8123/api/websocket", "Your Token here");

ha.Entity("input_boolean.dummy_switch").CallService("toggle");
Console.WriteLine("done");
