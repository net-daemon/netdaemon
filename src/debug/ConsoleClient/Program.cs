//#:package NetDaemon.HassModel@25.18.1

var ha = await NetDaemon.HassModel.HaContextFactory.CreateAsync("ws://localhost:8123/api/websocket", "your_token_here");

ha.Entity("input_boolean.dummy_switch").CallService("toggle");
Console.WriteLine("done");
