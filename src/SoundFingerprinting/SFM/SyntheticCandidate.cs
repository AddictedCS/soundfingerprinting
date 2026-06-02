namespace SoundFingerprinting.SFM;

using SoundFingerprinting.Query;

/// <summary>
///  A Phase-1 candidate emitted by an <see cref="ISfmMatchStrategy"/>.
/// </summary>
public sealed class SyntheticCandidate
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="SyntheticCandidate"/> class.
    /// </summary>
    /// <param name="queryMatchAt">Query position (seconds).</param>
    /// <param name="trackMatchAt">
    ///  Track position (seconds) on the local diagonal between this candidate's bracketing real matches,
    ///  computed by <see cref="SyntheticCandidateUtils"/> via linear interpolation (middle) or unit-rate
    ///  extrapolation from the single nearest anchor (head/tail).
    /// </param>
    /// <param name="matchType">The bridging strategy that produced this candidate.</param>
    public SyntheticCandidate(double queryMatchAt, double trackMatchAt, MatchedWithType matchType)
    {
        QueryMatchAt = queryMatchAt;
        TrackMatchAt = trackMatchAt;
        MatchedWithType = matchType;
    }

    /// <summary>
    ///  Gets the query-side time position (seconds) of the candidate.
    /// </summary>
    public double QueryMatchAt { get; }

    /// <summary>
    ///  Gets the track-side time position (seconds) of the candidate.
    /// </summary>
    public double TrackMatchAt { get; }

    /// <summary>
    ///  Gets the bridging strategy that produced this candidate.
    /// </summary>
    public MatchedWithType MatchedWithType { get; }
}
