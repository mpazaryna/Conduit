using Conduit.Core.Services;
using Conduit.Models;
using Microsoft.Extensions.Options;

namespace Conduit.Worker;

/// <summary>
/// A long-running background service that ingests data sources on a recurring schedule.
/// </summary>
/// <remarks>
/// <para>
/// Extends <see cref="BackgroundService"/>, the .NET standard base class for
/// long-running hosted services. The runtime calls <see cref="ExecuteAsync"/>
/// once at startup, and it runs until the application shuts down.
/// </para>
///
/// <para><b>Primary constructor syntax (C# 12):</b></para>
/// <para>
/// Parameters in the class declaration are injected by the DI container and
/// available throughout the class without explicit field declarations.
/// </para>
///
/// <para><b>CancellationToken:</b></para>
/// <para>
/// The <c>stoppingToken</c> is signaled on shutdown. We pass it to
/// <c>Task.Delay</c> so the service stops promptly. Always propagate
/// cancellation tokens in .NET async code.
/// </para>
/// </remarks>
/// <param name="adapter">The source adapter, resolved from DI.</param>
/// <param name="writer">The output writer, resolved from DI.</param>
/// <param name="settings">Typed configuration from appsettings.json.</param>
/// <param name="logger">Typed logger for this worker.</param>
public class Worker(
    ISourceAdapter adapter,
    IOutputWriter writer,
    IOptions<AppSettings> settings,
    ILogger<Worker> logger) : BackgroundService
{
    /// <summary>
    /// The main execution loop. Runs continuously until the application shuts down.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Pipeline starting");

            foreach (var source in settings.Value.Sources)
            {
                var items = await adapter.IngestAsync(source.Location);
                if (items.Count > 0)
                {
                    await writer.WriteAsync(items, source.Type, source.Name);
                }
            }

            logger.LogInformation("Pipeline complete. Next run in 5 minutes");
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
