using Conduit.Core.Models;

namespace Conduit.Core.Services;

/// <summary>
/// Defines the contract for persisting pipeline records to a storage backend.
/// </summary>
/// <remarks>
/// <para>
/// The current implementation (<c>JsonOutputWriter</c>) writes to the local
/// filesystem as JSON files. Alternative implementations could write to a
/// database, blob storage, or message queue without changing any calling code.
/// </para>
/// <para>
/// This is the <b>Strategy pattern</b> -- the "how" of persistence is a
/// pluggable strategy, selected at DI registration time.
/// </para>
/// </remarks>
public interface IOutputWriter
{
    /// <summary>
    /// Persists a collection of records organized by source type and name.
    /// </summary>
    /// <param name="items">The records to persist.</param>
    /// <param name="sourceType">
    /// The adapter type (e.g., "rss", "edi834"). Used to organize output
    /// into subdirectories by source type.
    /// </param>
    /// <param name="sourceName">
    /// A short identifier for the source (e.g., "hacker-news"). Used in
    /// the output filename.
    /// </param>
    /// <returns>A task that completes when the write operation finishes.</returns>
    Task WriteAsync(List<FeedItem> items, string sourceType, string sourceName);
}
