namespace Conduit.Core.Models;

/// <summary>
/// Base interface for all records flowing through the Conduit pipeline.
/// </summary>
/// <remarks>
/// <para>
/// Every source adapter produces records that implement this interface.
/// The pipeline loop, output writer, and other infrastructure operate on
/// <see cref="IPipelineRecord"/> without knowing the concrete type. Domain-specific
/// properties live on the implementing types (e.g., <see cref="FeedItem"/>).
/// </para>
/// <para>
/// This enables a single pipeline to process heterogeneous data -- RSS feeds,
/// EDI 834 enrollment files, or any future source -- without changing the
/// orchestration code.
/// </para>
/// </remarks>
public interface IPipelineRecord
{
    /// <summary>
    /// A unique identifier for this record within its source.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// When this record was created or published.
    /// </summary>
    DateTime Timestamp { get; }

    /// <summary>
    /// The type of source that produced this record (e.g., "rss", "edi834").
    /// </summary>
    string SourceType { get; }
}
