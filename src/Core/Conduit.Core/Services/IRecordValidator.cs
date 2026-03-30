using Conduit.Core.Models;

namespace Conduit.Core.Services;

/// <summary>
/// Defines a validation rule set for a specific record type.
/// </summary>
/// <remarks>
/// <para>
/// Each validator handles one record type. The <see cref="AppliesTo"/> guard
/// prevents a validator from running against records it doesn't understand.
/// The <see cref="Validate"/> method returns human-readable error messages —
/// an empty list means the record is valid.
/// </para>
/// <para>
/// Validators live in <c>Conduit.Transforms</c> where adapter model references
/// are available. This interface lives in <c>Conduit.Core</c> so the validation
/// transform can depend on it without pulling in adapters.
/// </para>
/// </remarks>
public interface IRecordValidator
{
    /// <summary>
    /// Returns true if this validator applies to the given record type.
    /// </summary>
    bool AppliesTo(IPipelineRecord record);

    /// <summary>
    /// Validates the record and returns a list of human-readable error messages.
    /// An empty list means the record is valid.
    /// </summary>
    IReadOnlyList<string> Validate(IPipelineRecord record);
}
