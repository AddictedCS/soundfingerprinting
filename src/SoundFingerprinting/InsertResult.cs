namespace SoundFingerprinting;

using System;
using ProtoBuf;

/// <summary>
///  Result of an <see cref="IModelService.Insert"/> call describing whether the track was persisted,
///  updated, or suppressed as a duplicate.
/// </summary>
[ProtoContract(SkipConstructor = true)]
public sealed record InsertResult
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="InsertResult"/> class.
    /// </summary>
    /// <param name="outcome">The insert outcome.</param>
    /// <param name="duplicateOfTrackId">
    ///  The id of the existing track this insert duplicated; mandatory when <paramref name="outcome"/>
    ///  is <see cref="InsertOutcome.DuplicateSuppressed"/>, otherwise <c>null</c>.
    /// </param>
    /// <exception cref="ArgumentException">
    ///  Thrown when <paramref name="outcome"/> is <see cref="InsertOutcome.DuplicateSuppressed"/> but
    ///  <paramref name="duplicateOfTrackId"/> is null or empty.
    /// </exception>
    public InsertResult(InsertOutcome outcome, string? duplicateOfTrackId = null)
    {
        if (outcome == InsertOutcome.DuplicateSuppressed && string.IsNullOrEmpty(duplicateOfTrackId))
        {
            throw new ArgumentException("DuplicateSuppressed requires a non-null DuplicateOfTrackId.", nameof(duplicateOfTrackId));
        }

        Outcome = outcome;
        DuplicateOfTrackId = duplicateOfTrackId;
    }

    /// <summary>
    ///  Gets the insert outcome.
    /// </summary>
    [ProtoMember(1)]
    public InsertOutcome Outcome { get; }

    /// <summary>
    ///  Gets the id of the existing track this insert duplicated, or <c>null</c> when the insert was not suppressed.
    /// </summary>
    [ProtoMember(2)]
    public string? DuplicateOfTrackId { get; }

    /// <summary>
    ///  Gets a value indicating whether the track ended up in storage. <see cref="InsertOutcome.Unknown"/> is
    ///  treated as persisted (optimistic backward-compatible fallback when talking to an older server).
    /// </summary>
    public bool Persisted => Outcome is InsertOutcome.Inserted or InsertOutcome.Updated or InsertOutcome.Unknown;

    /// <summary>
    ///  Gets a value indicating whether the insert was suppressed because the track is a full duplicate.
    /// </summary>
    public bool DuplicatePrevented => Outcome == InsertOutcome.DuplicateSuppressed;
}
