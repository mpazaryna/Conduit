using System.Text.Json;
using Microsoft.Extensions.Logging;
using Conduit.Core.Models;
using Conduit.Core.Services;

namespace Conduit.Services;

/// <summary>
/// Persists pipeline records as formatted JSON files on the local filesystem.
/// </summary>
/// <remarks>
/// <para>
/// Each call to <see cref="WriteAsync"/> creates a new timestamped file
/// (e.g., <c>hacker-news_2024-01-15_143022.json</c>). This append-only
/// approach preserves history and avoids overwriting previous outputs.
/// </para>
///
/// <para><b>Why static readonly for JsonOptions?</b></para>
/// <para>
/// <see cref="JsonSerializerOptions"/> is expensive to construct because it
/// caches type metadata internally. Creating it once as a <c>static readonly</c>
/// field means all calls share the same instance.
/// </para>
///
/// <para><b>Thread safety:</b></para>
/// <para>
/// This class is safe to register as a singleton in DI because
/// <see cref="JsonSerializer"/> is thread-safe when using a shared
/// <see cref="JsonSerializerOptions"/> instance (since .NET 8+).
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
    /// The directory where JSON files will be written. Created automatically
    /// if it does not exist.
    /// </param>
    /// <param name="logger">Typed logger for this component.</param>
    public JsonOutputWriter(string outputDir, ILogger<JsonOutputWriter> logger)
    {
        _outputDir = outputDir;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task WriteAsync(List<FeedItem> items, string sourceName)
    {
        Directory.CreateDirectory(_outputDir);

        var filename = $"{sourceName}_{DateTime.Now:yyyy-MM-dd_HHmmss}.json";
        var path = Path.Combine(_outputDir, filename);

        var json = JsonSerializer.Serialize(items, JsonOptions);
        await File.WriteAllTextAsync(path, json);

        _logger.LogInformation("Wrote {Count} items to {Path}", items.Count, path);
    }
}
