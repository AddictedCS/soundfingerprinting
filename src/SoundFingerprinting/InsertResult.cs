namespace SoundFingerprinting;

using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

/// <summary>
///  Result of an <see cref="IModelService.InsertAsync"/> call describing whether the track was persisted,
///  updated, suppressed as a duplicate, or left undecided for the client to resolve.
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
    /// <param name="undecidedResidentIds">
    ///  The residents the client has to decide against; mandatory and non-empty when <paramref name="outcome"/>
    ///  is <see cref="InsertOutcome.Undecided"/>, otherwise <c>null</c>.
    /// </param>
    /// <param name="sequence">
    ///  The server sequence this result reflects; mandatory when <paramref name="outcome"/> is
    ///  <see cref="InsertOutcome.Undecided"/>, optional otherwise.
    /// </param>
    /// <exception cref="ArgumentException">
    ///  Thrown when <paramref name="outcome"/> is <see cref="InsertOutcome.DuplicateSuppressed"/> but
    ///  <paramref name="duplicateOfTrackId"/> is null or empty, or when it is <see cref="InsertOutcome.Undecided"/>
    ///  without resident ids or without a sequence.
    /// </exception>
    public InsertResult(InsertOutcome outcome, string? duplicateOfTrackId = null, IReadOnlyList<string>? undecidedResidentIds = null, long? sequence = null)
    {
        if (outcome == InsertOutcome.DuplicateSuppressed && string.IsNullOrEmpty(duplicateOfTrackId))
        {
            throw new ArgumentException("DuplicateSuppressed requires a non-null DuplicateOfTrackId.", nameof(duplicateOfTrackId));
        }

        if (outcome == InsertOutcome.Undecided && (undecidedResidentIds == null || undecidedResidentIds.Count == 0))
        {
            throw new ArgumentException("Undecided requires at least one resident id.", nameof(undecidedResidentIds));
        }

        if (outcome == InsertOutcome.Undecided && sequence == null)
        {
            throw new ArgumentException("Undecided requires the server sequence.", nameof(sequence));
        }

        Outcome = outcome;
        DuplicateOfTrackId = duplicateOfTrackId;
        UndecidedResidentIds = undecidedResidentIds?.ToList();
        Sequence = sequence;
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
    ///  Gets the residents that were inserted after the request's snapshot and that the client has to decide against,
    ///  or <c>null</c> when the outcome is not <see cref="InsertOutcome.Undecided"/>.
    /// </summary>
    [ProtoMember(3)]
    public List<string>? UndecidedResidentIds { get; }

    /// <summary>
    ///  Gets the server sequence this result reflects: the snapshot for the next attempt after <see cref="InsertOutcome.Undecided"/>,
    ///  informational otherwise, <c>null</c> from a server that does not report it.
    /// </summary>
    [ProtoMember(4)]
    public long? Sequence { get; }

    /// <summary>
    ///  Gets a value indicating whether the track ended up in storage. <see cref="InsertOutcome.Unknown"/> is
    ///  treated as persisted (optimistic backward-compatible fallback when talking to an older server).
    /// </summary>
    public bool Persisted => Outcome is InsertOutcome.Inserted or InsertOutcome.Updated or InsertOutcome.Unknown;

    /// <summary>
    ///  Gets a value indicating whether the insert was suppressed because the track is a full duplicate.
    /// </summary>
    public bool DuplicatePrevented => Outcome == InsertOutcome.DuplicateSuppressed;

    /// <summary>
    ///  Gets a value indicating whether nothing was written because undecided residents arrived after the request's snapshot.
    /// </summary>
    public bool Undecided => Outcome == InsertOutcome.Undecided;
}
