using Conduit.Core.Models;

namespace Conduit.Core.Services;

/// <summary>
/// Defines the contract for ingesting data from an external source.
/// </summary>
/// <remarks>
/// <para>
/// Each source type (RSS, Atom, EDI 834, REST API) implements this interface.
/// The pipeline doesn't know or care which adapter it's using -- it calls
/// <see cref="IngestAsync"/> and gets records back.
/// </para>
/// <para>
/// This is the <b>Open/Closed Principle</b> in action: adding a new source
/// type means implementing this interface, not modifying the pipeline.
/// </para>
/// </remarks>
public interface ISourceAdapter
{
    /// <summary>
    /// Ingests data from the configured source and returns parsed records.
    /// </summary>
    /// <param name="location">
    /// The source location -- a URL for feeds, a file path for batch files,
    /// or any identifier the adapter understands.
    /// </param>
    /// <returns>
    /// A list of parsed <see cref="FeedItem"/> records. Returns an empty list
    /// if the ingestion fails or the source contains no data.
    /// </returns>
    Task<List<FeedItem>> IngestAsync(string location);
}
