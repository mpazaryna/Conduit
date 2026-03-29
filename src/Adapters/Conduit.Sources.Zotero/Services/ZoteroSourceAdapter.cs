using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Conduit.Core.Models;
using Conduit.Core.Services;
using Conduit.Sources.Zotero.Models;

namespace Conduit.Sources.Zotero.Services;

/// <summary>
/// Ingests and parses Zotero CSV export files from disk, with optional
/// arxiv API enrichment for papers hosted on arxiv.org.
/// </summary>
/// <remarks>
/// <para>
/// Reads the standard Zotero CSV export format, finding columns by header
/// name (Title, Author, DOI, Url, Abstract Note, Manual Tags). This handles
/// both minimal exports and full exports with 80+ columns.
/// </para>
/// <para>
/// For entries with arxiv URLs, the adapter calls the arxiv API to fetch
/// the abstract. Access level is inferred: papers with an abstract are
/// <see cref="AccessLevel.Open"/>, papers with a DOI but no abstract are
/// <see cref="AccessLevel.Paywalled"/>, and all others are
/// <see cref="AccessLevel.Unknown"/>.
/// </para>
/// </remarks>
public partial class ZoteroSourceAdapter : ISourceAdapter
{
    private static readonly XNamespace AtomNs = "http://www.w3.org/2005/Atom";

    private readonly HttpClient _httpClient;
    private readonly ILogger<ZoteroSourceAdapter> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ZoteroSourceAdapter"/>.
    /// </summary>
    /// <param name="httpClient">HTTP client for arxiv API calls.</param>
    /// <param name="logger">Typed logger for structured logging.</param>
    public ZoteroSourceAdapter(HttpClient httpClient, ILogger<ZoteroSourceAdapter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<IPipelineRecord>> IngestAsync(string location)
    {
        _logger.LogInformation("Ingesting Zotero CSV source: {Location}", location);

        try
        {
            if (!File.Exists(location))
            {
                _logger.LogError("Zotero CSV file not found: {Location}", location);
                return [];
            }

            var lines = await File.ReadAllLinesAsync(location);

            if (lines.Length <= 1)
            {
                _logger.LogInformation("Zotero CSV is empty or header-only: {Location}", location);
                return [];
            }

            // Strip BOM if present and build column index from header row
            var headerLine = lines[0].TrimStart('\uFEFF');
            var headers = ParseCsvLine(headerLine);
            var columnMap = BuildColumnMap(headers);

            var records = new List<ResearchRecord>();

            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var fields = ParseCsvLine(line);

                var title = GetField(fields, columnMap, "Title");
                var authors = GetField(fields, columnMap, "Author");
                var doi = GetField(fields, columnMap, "DOI");
                var url = GetField(fields, columnMap, "Url");
                var abstractNote = GetField(fields, columnMap, "Abstract Note");
                var tags = GetField(fields, columnMap, "Manual Tags");
                var arxivId = ExtractArxivId(url);

                if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(url))
                {
                    continue;
                }

                records.Add(new ResearchRecord(
                    Title: title,
                    Authors: authors,
                    Doi: doi,
                    Url: url,
                    Abstract: abstractNote,
                    Tags: tags,
                    AccessLevel: AccessLevel.Unknown,
                    ArxivId: arxivId));
            }

            // Enrich arxiv entries
            await EnrichArxivRecords(records);

            // Determine access levels after enrichment
            var result = records.Select(r => (IPipelineRecord)DetermineAccessLevel(r)).ToList();

            _logger.LogInformation("Parsed {Count} research records from {Location}",
                result.Count, location);
            return result;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Failed to parse Zotero CSV: {Location}", location);
            return [];
        }
    }

    /// <summary>
    /// Builds a mapping from column name to index from the header row.
    /// </summary>
    private static Dictionary<string, int> BuildColumnMap(List<string> headers)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < headers.Count; i++)
        {
            var name = headers[i].Trim();
            if (!string.IsNullOrWhiteSpace(name) && !map.ContainsKey(name))
            {
                map[name] = i;
            }
        }
        return map;
    }

    /// <summary>
    /// Gets a field value by column name, returning empty string if the
    /// column doesn't exist or the row is too short.
    /// </summary>
    private static string GetField(List<string> fields, Dictionary<string, int> columnMap, string columnName)
    {
        if (!columnMap.TryGetValue(columnName, out var index) || index >= fields.Count)
        {
            return "";
        }
        return fields[index].Trim();
    }

    /// <summary>
    /// Parses a CSV line respecting quoted fields.
    /// </summary>
    internal static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var inQuotes = false;
        var current = new System.Text.StringBuilder();

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());
        return fields;
    }

    /// <summary>
    /// Extracts the arxiv paper ID from a URL containing arxiv.org.
    /// </summary>
    internal static string ExtractArxivId(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return "";
        }

        var match = ArxivIdPattern().Match(url);
        return match.Success ? match.Groups[1].Value : "";
    }

    [GeneratedRegex(@"arxiv\.org/(?:abs|pdf)/(\d+\.\d+)")]
    private static partial Regex ArxivIdPattern();

    private async Task EnrichArxivRecords(List<ResearchRecord> records)
    {
        for (var i = 0; i < records.Count; i++)
        {
            var record = records[i];
            if (string.IsNullOrWhiteSpace(record.ArxivId) || !string.IsNullOrWhiteSpace(record.Abstract))
            {
                continue;
            }

            try
            {
                var apiUrl = $"https://export.arxiv.org/api/query?id_list={record.ArxivId}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Arxiv API returned {StatusCode} for {ArxivId}",
                        response.StatusCode, record.ArxivId);
                    continue;
                }

                var xml = await response.Content.ReadAsStringAsync();
                var doc = XDocument.Parse(xml);

                var summary = doc.Descendants(AtomNs + "entry")
                    .FirstOrDefault()
                    ?.Element(AtomNs + "summary")
                    ?.Value.Trim();

                if (!string.IsNullOrWhiteSpace(summary))
                {
                    records[i] = record with { Abstract = summary };
                }
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                _logger.LogWarning(ex, "Failed to fetch arxiv abstract for {ArxivId}", record.ArxivId);
            }
        }
    }

    private static ResearchRecord DetermineAccessLevel(ResearchRecord record)
    {
        if (!string.IsNullOrWhiteSpace(record.Abstract))
        {
            return record with { AccessLevel = AccessLevel.Open };
        }

        if (!string.IsNullOrWhiteSpace(record.Doi))
        {
            return record with { AccessLevel = AccessLevel.Paywalled };
        }

        return record with { AccessLevel = AccessLevel.Unknown };
    }
}
