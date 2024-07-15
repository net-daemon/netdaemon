using NetDaemon.Tests.Performance;
using Serilog;

#pragma warning disable CA1812

// This assembly is not used by the host so we have to force load it
// so it will be available for source deployment scenarios

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .UseSerilog((context, provider, logConfig) =>
    {
      logConfig.ReadFrom.Configuration(context.Configuration);
    });
builder.Services.AddScoped<PerfServerStartup>();
var app = builder.Build();

app.UsePerfServerWebsockets();

try
{
    await app.RunAsync().ConfigureAwait(false);
}
catch (OperationCanceledException) { } // Ignore
catch (Exception e)
{
    Console.WriteLine($"Failed to start host... {e}");
    throw;
}
