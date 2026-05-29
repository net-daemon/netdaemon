using NetDaemon.Tests.Performance;

#pragma warning disable CA1812


var builder = WebApplication.CreateBuilder(args);

var app = builder.UsePerfServerSettings()
    .Build();

app.AddPerfServer();

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
