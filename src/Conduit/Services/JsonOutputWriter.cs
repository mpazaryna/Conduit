using System.Text.Json;
using Microsoft.Extensions.Logging;
using Conduit.Core.Models;
using Conduit.Core.Services;

namespace Conduit.Services;

/// <summary>
/// Persists pipeline records as formatted JSON files on the local filesystem,
/// organized by source type.
/// </summary>
/// <remarks>
/// <para>
/// Output is organized as <c>data/{sourceType}/{sourceName}_{timestamp}.json</c>.
/// Each source type gets its own subdirectory, making it easy to see which
/// adapters have produced output.
/// </para>
/// </remarks>
public class JsonOutputWriter : IOutputWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _outputDir;
    private readonly ILogger<JsonOutputWriter> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="JsonOutputWriter"/>.
    /// </summary>
    /// <param name="outputDir">
    /// The root output directory (e.g., "data"). Source type subdirectories
    /// are created automatically beneath it.
    /// </param>
    /// <param name="logger">Typed logger for this component.</param>
    public JsonOutputWriter(string outputDir, ILogger<JsonOutputWriter> logger)
    {
        _outputDir = outputDir;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task WriteAsync(List<FeedItem> items, string sourceType, string sourceName)
    {
        var typeDir = Path.Combine(_outputDir, sourceType);
        Directory.CreateDirectory(typeDir);

        var filename = $"{sourceName}_{DateTime.Now:yyyy-MM-dd_HHmmss}.json";
        var path = Path.Combine(typeDir, filename);

        var json = JsonSerializer.Serialize(items, JsonOptions);
        await File.WriteAllTextAsync(path, json);

        _logger.LogInformation("Wrote {Count} items to {Path}", items.Count, path);
    }
}
