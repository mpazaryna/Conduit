namespace Conduit.Sources.Zotero.Models;

/// <summary>
/// Indicates whether a research paper's full text is freely accessible.
/// </summary>
public enum AccessLevel
{
    /// <summary>The paper has an abstract available and is presumed openly accessible.</summary>
    Open,

    /// <summary>The paper has a DOI but no abstract, suggesting it may be behind a paywall.</summary>
    Paywalled,

    /// <summary>Access level could not be determined from the available metadata.</summary>
    Unknown
}
