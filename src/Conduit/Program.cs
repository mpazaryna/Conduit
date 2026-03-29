// -----------------------------------------------------------------------
// Conduit Console Runner
//
// One-shot console app that ingests all configured sources, writes the
// results to disk, and exits.
//
// ARCHITECTURE NOTES:
//
// 1. Configuration -- Loaded from appsettings.json. AppContext.BaseDirectory
//    ensures the file is found relative to the compiled output.
//
// 2. Logging -- Serilog with Console + File sinks. Daily rolling log files.
//
// 3. Dependency Injection -- ServiceCollection wires up ISourceAdapter and
//    IOutputWriter. AddHttpClient manages HttpClient lifecycle.
//
// 4. Pipeline Loop -- Iterates over each configured source, ingests data,
//    and writes non-empty results to disk. Errors in one source don't stop
//    the others (handled inside the adapter).
//
// RUN WITH: dotnet run --project src/Conduit
// -----------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Conduit.Core.Services;
using Conduit.Models;
using Conduit.Services;

// -- Load configuration from appsettings.json --
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var appSettings = configuration.GetSection("App").Get<AppSettings>()
    ?? throw new InvalidOperationException("Missing 'App' section in appsettings.json");

// -- Configure Serilog --
Directory.CreateDirectory(appSettings.LogsDir);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        Path.Combine(appSettings.LogsDir, "log-.txt"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

// -- Register services in DI container --
var services = new ServiceCollection();

services.AddLogging(builder => builder.AddSerilog(Log.Logger));
services.AddHttpClient<ISourceAdapter, RssSourceAdapter>();
services.AddSingleton<IOutputWriter>(sp =>
    new JsonOutputWriter(appSettings.OutputDir, sp.GetRequiredService<ILogger<JsonOutputWriter>>()));

var provider = services.BuildServiceProvider();

// -- Run the pipeline --
var logger = provider.GetRequiredService<ILogger<Program>>();
var adapter = provider.GetRequiredService<ISourceAdapter>();
var writer = provider.GetRequiredService<IOutputWriter>();

logger.LogInformation("Starting pipeline");

foreach (var source in appSettings.Sources)
{
    var items = await adapter.IngestAsync(source.Location);
    if (items.Count > 0)
    {
        await writer.WriteAsync(items, source.Name);
    }
}

logger.LogInformation("Pipeline complete");
Log.CloseAndFlush();
