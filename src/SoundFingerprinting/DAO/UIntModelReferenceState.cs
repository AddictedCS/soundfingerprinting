namespace SoundFingerprinting.DAO;

/// <summary>
///  A consistent snapshot of the counters of a <see cref="UIntModelReferenceTracker"/>.
/// </summary>
/// <param name="Lap">Count of completed wraps.</param>
/// <param name="TrackRef">Last issued track reference.</param>
/// <param name="SubFingerprintRef">Last issued sub-fingerprint reference.</param>
/// <remarks>
///  The three values come from one atomic read. A caller that persists the write head needs the lap together with
///  the references: after a wrap the references alone do not tell which lap issued them.
/// </remarks>
public sealed record UIntModelReferenceState(long Lap, long TrackRef, long SubFingerprintRef)
{
    /// <summary>
    ///  Gets the count of wraps that the tracker completed since construction. Stays 0 when the tracker does not wrap.
    /// </summary>
    public long Lap { get; } = Lap;

    /// <summary>
    ///  Gets the last track reference that the tracker issued.
    /// </summary>
    public long TrackRef { get; } = TrackRef;

    /// <summary>
    ///  Gets the last sub-fingerprint reference that the tracker issued.
    /// </summary>
    public long SubFingerprintRef { get; } = SubFingerprintRef;
}
