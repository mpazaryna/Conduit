// -----------------------------------------------------------------------
// ZoteroSourceAdapter Tests
//
// Unit tests for the Zotero CSV source adapter. Tests verify CSV parsing,
// field extraction, AccessLevel detection, arxiv ID extraction, and
// error handling for missing/empty/malformed files.
// -----------------------------------------------------------------------

using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Conduit.Core.Models;
using Conduit.Core.Services;
using Conduit.Sources.Zotero.Models;
using Conduit.Sources.Zotero.Services;
using Moq;
using Moq.Protected;

namespace Conduit.Sources.Zotero.Tests;

/// <summary>
/// Tests for <see cref="ZoteroSourceAdapter"/> CSV parsing, enrichment, and error handling.
/// </summary>
public class ZoteroSourceAdapterTests
{
    private static readonly string FixturesDir = Path.Combine(AppContext.BaseDirectory, "fixtures");

    private static HttpClient CreateMockHttpClient(
        Func<HttpRequestMessage, HttpResponseMessage>? handler = null)
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
                handler?.Invoke(req) ?? new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("<feed></feed>", Encoding.UTF8, "application/xml")
                });

        return new HttpClient(mock.Object);
    }

    private static ZoteroSourceAdapter CreateAdapter(HttpClient? httpClient = null)
    {
        return new ZoteroSourceAdapter(
            httpClient ?? CreateMockHttpClient(),
            NullLogger<ZoteroSourceAdapter>.Instance);
    }

    // ---- CSV PARSING TESTS ----

    [Fact]
    public async Task IngestAsync_ParsesCsvRows()
    {
        var adapter = CreateAdapter();
        var path = Path.Combine(FixturesDir, "sample-zotero.csv");

        var items = await adapter.IngestAsync(path);

        Assert.Equal(4, items.Count);
    }

    [Fact]
    public async Task IngestAsync_ExtractsTitle()
    {
        var adapter = CreateAdapter();
        var path = Path.Combine(FixturesDir, "sample-zotero.csv");

        var items = await adapter.IngestAsync(path);
        var records = items.Cast<ResearchRecord>().ToList();

        Assert.Equal("Attention Is All You Need", records[0].Title);
        Assert.Equal("Deep Residual Learning for Image Recognition", records[1].Title);
    }

    [Fact]
    public async Task IngestAsync_ExtractsAuthors()
    {
        var adapter = CreateAdapter();
        var path = Path.Combine(FixturesDir, "sample-zotero.csv");

        var items = await adapter.IngestAsync(path);
        var records = items.Cast<ResearchRecord>().ToList();

        Assert.Equal("Vaswani, Ashish; Shazeer, Noam; Parmar, Niki", records[0].Authors);
        Assert.Equal("He, Kaiming; Zhang, Xiangyu", records[1].Authors);
    }

    [Fact]
    public async Task IngestAsync_ExtractsDoi()
    {
        var adapter = CreateAdapter();
        var path = Path.Combine(FixturesDir, "sample-zotero.csv");

        var items = await adapter.IngestAsync(path);
        var records = items.Cast<ResearchRecord>().ToList();

        Assert.Equal("", records[0].Doi);
        Assert.Equal("10.1109/CVPR.2016.90", records[1].Doi);
        Assert.Equal("10.18653/v1/N19-1423", records[2].Doi);
    }

    [Fact]
    public async Task IngestAsync_ExtractsUrl()
    {
        var adapter = CreateAdapter();
        var path = Path.Combine(FixturesDir, "sample-zotero.csv");

        var items = await adapter.IngestAsync(path);
        var records = items.Cast<ResearchRecord>().ToList();

        Assert.Equal("https://arxiv.org/abs/1706.03762", records[0].Url);
        Assert.Equal("https://ieeexplore.ieee.org/document/7780459", records[1].Url);
    }

    [Fact]
    public async Task IngestAsync_ExtractsTags()
    {
        var adapter = CreateAdapter();
        var path = Path.Combine(FixturesDir, "sample-zotero.csv");

        var items = await adapter.IngestAsync(path);
        var records = items.Cast<ResearchRecord>().ToList();

        Assert.Equal("transformers; deep-learning", records[0].Tags);
        Assert.Equal("computer-vision; residual-networks", records[1].Tags);
    }

    [Fact]
    public async Task IngestAsync_ExtractsAbstract()
    {
        var adapter = CreateAdapter();
        var path = Path.Combine(FixturesDir, "sample-zotero.csv");

        var items = await adapter.IngestAsync(path);
        var records = items.Cast<ResearchRecord>().ToList();

        Assert.Contains("BERT", records[2].Abstract);
    }

    // ---- ID TESTS ----

    [Fact]
    public async Task IngestAsync_UsesDoiAsIdWhenPresent()
    {
        var adapter = CreateAdapter();
        var path = Path.Combine(FixturesDir, "sample-zotero.csv");

        var items = await adapter.IngestAsync(path);
        var records = items.Cast<ResearchRecord>().ToList();

        Assert.Equal("10.1109/CVPR.2016.90", records[1].Id);
    }

    [Fact]
    public async Task IngestAsync_UsesUrlAsIdWhenNoDoiPresent()
    {
        var adapter = CreateAdapter();
        var path = Path.Combine(FixturesDir, "sample-zotero.csv");

        var items = await adapter.IngestAsync(path);
        var records = items.Cast<ResearchRecord>().ToList();

        Assert.Equal("https://arxiv.org/abs/1706.03762", records[0].Id);
    }

    [Fact]
    public async Task IngestAsync_SourceTypeIsZotero()
    {
        var adapter = CreateAdapter();
        var path = Path.Combine(FixturesDir, "sample-zotero.csv");

        var items = await adapter.IngestAsync(path);

        Assert.All(items, item => Assert.Equal("zotero", item.SourceType));
    }

    // ---- ACCESS LEVEL TESTS ----

    [Fact]
    public async Task IngestAsync_SetsOpenWhenAbstractPresent()
    {
        var adapter = CreateAdapter();
        var path = Path.Combine(FixturesDir, "sample-zotero.csv");

        var items = await adapter.IngestAsync(path);
        var records = items.Cast<ResearchRecord>().ToList();

        // BERT entry has an abstract in the CSV
        Assert.Equal(AccessLevel.Open, records[2].AccessLevel);
    }

    [Fact]
    public async Task IngestAsync_SetsPaywalledWhenDoiButNoAbstract()
    {
        var adapter = CreateAdapter();
        var path = Path.Combine(FixturesDir, "sample-zotero.csv");

        var items = await adapter.IngestAsync(path);
        var records = items.Cast<ResearchRecord>().ToList();

        // ResNet entry has a DOI but no abstract (and no arxiv URL)
        Assert.Equal(AccessLevel.Paywalled, records[1].AccessLevel);
    }

    [Fact]
    public async Task IngestAsync_SetsUnknownWhenNoDoiAndNoAbstract()
    {
        var adapter = CreateAdapter();
        var path = Path.Combine(FixturesDir, "sample-zotero.csv");

        var items = await adapter.IngestAsync(path);
        var records = items.Cast<ResearchRecord>().ToList();

        // "A Brief History" has no DOI and no abstract
        Assert.Equal(AccessLevel.Unknown, records[3].AccessLevel);
    }

    // ---- ARXIV ID EXTRACTION TESTS ----

    [Fact]
    public async Task IngestAsync_ExtractsArxivIdFromUrl()
    {
        var adapter = CreateAdapter();
        var path = Path.Combine(FixturesDir, "sample-zotero.csv");

        var items = await adapter.IngestAsync(path);
        var records = items.Cast<ResearchRecord>().ToList();

        Assert.Equal("1706.03762", records[0].ArxivId);
    }

    [Fact]
    public async Task IngestAsync_ArxivIdIsEmptyForNonArxivUrls()
    {
        var adapter = CreateAdapter();
        var path = Path.Combine(FixturesDir, "sample-zotero.csv");

        var items = await adapter.IngestAsync(path);
        var records = items.Cast<ResearchRecord>().ToList();

        Assert.Equal("", records[1].ArxivId);
        Assert.Equal("", records[3].ArxivId);
    }

    // ---- ARXIV ENRICHMENT TESTS ----

    [Fact]
    public async Task IngestAsync_CallsArxivApiForArxivEntries()
    {
        var arxivResponse = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <entry>
                <summary>The dominant sequence transduction models are based on complex recurrent or convolutional neural networks.</summary>
              </entry>
            </feed>
            """;

        var requestedUrls = new List<string>();
        var httpClient = CreateMockHttpClient(req =>
        {
            requestedUrls.Add(req.RequestUri?.ToString() ?? "");
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(arxivResponse, Encoding.UTF8, "application/xml")
            };
        });

        var adapter = CreateAdapter(httpClient);
        var path = Path.Combine(FixturesDir, "sample-zotero.csv");

        var items = await adapter.IngestAsync(path);
        var records = items.Cast<ResearchRecord>().ToList();

        // Should have called the arxiv API for the arxiv entry
        Assert.Contains(requestedUrls, url => url.Contains("1706.03762"));

        // Should have populated the abstract from the API response
        Assert.Contains("dominant sequence transduction", records[0].Abstract);
    }

    [Fact]
    public async Task IngestAsync_SetsOpenAccessLevelAfterArxivEnrichment()
    {
        var arxivResponse = """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <entry>
                <summary>Some abstract text from arxiv.</summary>
              </entry>
            </feed>
            """;

        var httpClient = CreateMockHttpClient(_ => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(arxivResponse, Encoding.UTF8, "application/xml")
        });

        var adapter = CreateAdapter(httpClient);
        var path = Path.Combine(FixturesDir, "sample-zotero.csv");

        var items = await adapter.IngestAsync(path);
        var records = items.Cast<ResearchRecord>().ToList();

        // Arxiv entry should be Open after enrichment
        Assert.Equal(AccessLevel.Open, records[0].AccessLevel);
    }

    [Fact]
    public async Task IngestAsync_HandlesArxivApiFailureGracefully()
    {
        var httpClient = CreateMockHttpClient(_ => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        });

        var adapter = CreateAdapter(httpClient);
        var path = Path.Combine(FixturesDir, "sample-zotero.csv");

        var items = await adapter.IngestAsync(path);

        // Should still return all records, just without enrichment
        Assert.Equal(4, items.Count);
    }

    // ---- ERROR HANDLING TESTS ----

    [Fact]
    public async Task IngestAsync_ReturnsEmptyForMissingFile()
    {
        var adapter = CreateAdapter();

        var items = await adapter.IngestAsync("/nonexistent/path/to/file.csv");

        Assert.Empty(items);
    }

    [Fact]
    public async Task IngestAsync_ReturnsEmptyForEmptyFile()
    {
        var emptyFile = Path.Combine(FixturesDir, "empty.csv");
        await File.WriteAllTextAsync(emptyFile, "");

        try
        {
            var adapter = CreateAdapter();
            var items = await adapter.IngestAsync(emptyFile);

            Assert.Empty(items);
        }
        finally
        {
            File.Delete(emptyFile);
        }
    }

    [Fact]
    public async Task IngestAsync_ReturnsEmptyForHeaderOnlyFile()
    {
        var headerOnly = Path.Combine(FixturesDir, "header-only.csv");
        await File.WriteAllTextAsync(headerOnly,
            "\"Title\",\"Author\",\"DOI\",\"Url\",\"Abstract Note\",\"Manual Tags\"\n");

        try
        {
            var adapter = CreateAdapter();
            var items = await adapter.IngestAsync(headerOnly);

            Assert.Empty(items);
        }
        finally
        {
            File.Delete(headerOnly);
        }
    }

    [Fact]
    public async Task IngestAsync_SkipsMalformedRows()
    {
        var malformedFile = Path.Combine(FixturesDir, "malformed.csv");
        var content = """
            "Title","Author","DOI","Url","Abstract Note","Manual Tags"
            "Good Paper","Author A","10.1234/test","https://example.com","An abstract","tag1"
            "Bad Row"
            "Another Good","Author B","","https://example.com/2","","tag2"
            """;
        await File.WriteAllTextAsync(malformedFile, content);

        try
        {
            var adapter = CreateAdapter();
            var items = await adapter.IngestAsync(malformedFile);

            Assert.Equal(2, items.Count);
        }
        finally
        {
            File.Delete(malformedFile);
        }
    }
}
