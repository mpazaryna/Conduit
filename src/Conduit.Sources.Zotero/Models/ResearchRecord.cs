using Conduit.Core.Models;

namespace Conduit.Sources.Zotero.Models;

/// <summary>
/// Represents a single research paper parsed from a Zotero CSV export.
/// </summary>
/// <remarks>
/// <para>
/// Maps columns from Zotero's CSV export format into a pipeline record.
/// The <see cref="Id"/> is derived from the DOI when available, falling
/// back to the URL. The <see cref="AccessLevel"/> is inferred from the
/// presence of an abstract and DOI.
/// </para>
/// </remarks>
/// <param name="Title">The title of the paper.</param>
/// <param name="Authors">Semicolon-separated list of authors.</param>
/// <param name="Doi">The Digital Object Identifier, if available.</param>
/// <param name="Url">The URL where the paper can be accessed.</param>
/// <param name="Abstract">The paper's abstract text.</param>
/// <param name="Tags">Semicolon-separated list of manual tags from Zotero.</param>
/// <param name="AccessLevel">Whether the paper is open, paywalled, or unknown.</param>
/// <param name="ArxivId">The arxiv identifier extracted from the URL, if applicable.</param>
public record ResearchRecord(
    string Title,
    string Authors,
    string Doi,
    string Url,
    string Abstract,
    string Tags,
    AccessLevel AccessLevel,
    string ArxivId
) : IPipelineRecord
{
    /// <inheritdoc />
    public string Id => !string.IsNullOrWhiteSpace(Doi) ? Doi : Url;

    /// <inheritdoc />
    public DateTime Timestamp => DateTime.UtcNow;

    /// <inheritdoc />
    public string SourceType => "zotero";
}
