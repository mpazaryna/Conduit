// -----------------------------------------------------------------------
// JsonOutputWriter Tests
//
// Unit tests for the JSON file writing logic. These tests verify that
// JsonOutputWriter correctly serializes records, organizes output by
// source type, and manages output files.
//
// Each test creates a unique temporary directory (via Guid) and cleans
// it up in Dispose().
// -----------------------------------------------------------------------

using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Conduit.Core.Models;
using Conduit.Core.Services;
using Conduit.Services;

namespace Conduit.Tests;

/// <summary>
/// Tests for <see cref="JsonOutputWriter"/> file output behavior.
/// </summary>
public class JsonOutputWriterTests : IDisposable
{
    private readonly string _tempDir;

    /// <summary>Initializes a fresh temp directory for each test.</summary>
    public JsonOutputWriterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"conduit-tests-{Guid.NewGuid()}");
    }

    /// <summary>
    /// Cleans up the temporary directory after each test completes.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task WriteAsync_CreatesSourceTypeSubdirectory()
    {
        var writer = new JsonOutputWriter(_tempDir, NullLogger<JsonOutputWriter>.Instance);

        await writer.WriteAsync([], "rss", "test-source");

        Assert.True(Directory.Exists(Path.Combine(_tempDir, "rss")));
    }

    [Fact]
    public async Task WriteAsync_WritesJsonFileInTypeDirectory()
    {
        var writer = new JsonOutputWriter(_tempDir, NullLogger<JsonOutputWriter>.Instance);
        var items = new List<IPipelineRecord>
        {
            new FeedItem("Test Title", "https://example.com", "A description", new DateTime(2024, 1, 1))
        };

        await writer.WriteAsync(items, "rss", "test-source");

        var files = Directory.GetFiles(Path.Combine(_tempDir, "rss"), "*.json");
        Assert.Single(files);
    }

    [Fact]
    public async Task WriteAsync_FileContainsCorrectData()
    {
        var writer = new JsonOutputWriter(_tempDir, NullLogger<JsonOutputWriter>.Instance);
        var items = new List<IPipelineRecord>
        {
            new FeedItem("Test Title", "https://example.com", "A description", new DateTime(2024, 1, 1))
        };

        await writer.WriteAsync(items, "rss", "test-source");

        var file = Directory.GetFiles(Path.Combine(_tempDir, "rss"), "*.json").Single();
        var json = await File.ReadAllTextAsync(file);
        var deserialized = JsonSerializer.Deserialize<List<FeedItem>>(json);

        Assert.NotNull(deserialized);
        Assert.Single(deserialized);
        Assert.Equal("Test Title", deserialized[0].Title);
        Assert.Equal("https://example.com", deserialized[0].Link);
    }

    [Fact]
    public async Task WriteAsync_FilenameContainsSourceName()
    {
        var writer = new JsonOutputWriter(_tempDir, NullLogger<JsonOutputWriter>.Instance);

        await writer.WriteAsync([], "rss", "my-source");

        var file = Path.GetFileName(Directory.GetFiles(Path.Combine(_tempDir, "rss")).Single());
        Assert.StartsWith("my-source_", file);
        Assert.EndsWith(".json", file);
    }
}
