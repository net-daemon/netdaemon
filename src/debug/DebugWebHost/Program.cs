using NetDaemon.HassModel;
using NetDaemon.Runtime;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseNetDaemonRuntime();

var app = builder.Build();

app.MapGet("/", (IHaContext ha) => ha.GetAllEntities().Where(e=>e.EntityId.StartsWith("light")).Select(e => e.EntityId));
app.MapGet("/Off", (IHaContext ha) => ha.Entity("light.spots_woonkamer_rechts").CallService("turn_off"));
app.MapGet("/On", (IHaContext ha) => ha.Entity("light.spots_woonkamer_rechts").CallService("turn_on"));
app.MapGet("/State/{id}", (IHaContext ha, string id) => ha.Entity(id).EntityState);

app.MapGet("/Stop", () => app.StopAsync());

app.Run();

