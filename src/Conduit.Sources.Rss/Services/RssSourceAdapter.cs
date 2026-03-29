using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Conduit.Core.Models;
using Conduit.Core.Services;

namespace Conduit.Sources.Rss.Services;

/// <summary>
/// Ingests and parses RSS 2.0 feeds over HTTP.
/// </summary>
/// <remarks>
/// <para>
/// This is a concrete implementation of <see cref="ISourceAdapter"/> for RSS feeds.
/// It uses <see cref="HttpClient"/> for HTTP requests and <see cref="XDocument"/>
/// (LINQ to XML) for parsing the RSS XML.
/// </para>
///
/// <para><b>Dependency Injection:</b></para>
/// <para>
/// Both dependencies (<c>HttpClient</c> and <c>ILogger</c>) are received through
/// the constructor via <b>constructor injection</b>. The DI container creates the
/// object and supplies its dependencies automatically.
/// </para>
///
/// <para><b>HttpClient lifecycle:</b></para>
/// <para>
/// The <c>HttpClient</c> is provided by <c>IHttpClientFactory</c> (registered via
/// <c>AddHttpClient&lt;ISourceAdapter, RssSourceAdapter&gt;()</c> in Program.cs).
/// This avoids socket exhaustion under load.
/// </para>
///
/// <para><b>Error handling strategy:</b></para>
/// <para>
/// Network errors (<see cref="HttpRequestException"/>) and malformed XML
/// (<see cref="System.Xml.XmlException"/>) are caught, logged, and converted to
/// empty results. This allows the pipeline to continue processing other sources
/// even when one source is down or returns invalid content.
/// </para>
/// </remarks>
public class RssSourceAdapter : ISourceAdapter
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RssSourceAdapter> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="RssSourceAdapter"/>.
    /// </summary>
    /// <param name="httpClient">
    /// The HTTP client used to fetch feed content. Managed by IHttpClientFactory.
    /// </param>
    /// <param name="logger">Typed logger for structured logging.</param>
    public RssSourceAdapter(HttpClient httpClient, ILogger<RssSourceAdapter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<FeedItem>> IngestAsync(string location)
    {
        _logger.LogInformation("Ingesting RSS source: {Location}", location);

        try
        {
            var response = await _httpClient.GetStringAsync(location);
            var doc = XDocument.Parse(response);

            var items = doc.Descendants("item")
                .Select(item => new FeedItem(
                    Title: item.Element("title")?.Value ?? "(no title)",
                    Link: item.Element("link")?.Value ?? "",
                    Description: StripHtml(item.Element("description")?.Value ?? ""),
                    PublishedDate: DateTime.TryParse(item.Element("pubDate")?.Value, out var date)
                        ? date
                        : DateTime.MinValue
                ))
                .ToList();

            _logger.LogInformation("Parsed {Count} items from {Location}", items.Count, location);
            return items;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch source: {Location}", location);
            return [];
        }
        catch (System.Xml.XmlException ex)
        {
            _logger.LogError(ex, "Failed to parse source XML: {Location}", location);
            return [];
        }
    }

    /// <summary>
    /// Removes HTML tags from a string, leaving only plain text.
    /// </summary>
    private static string StripHtml(string html)
    {
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", "").Trim();
    }
}
