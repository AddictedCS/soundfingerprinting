namespace SoundFingerprinting.SFM;

using System.Collections.Generic;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Query;

/// <summary>
///  Context passed to <see cref="ISfmMatchStrategy.GenerateCandidates"/>.
/// </summary>
public sealed class SyntheticMatchContext
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="SyntheticMatchContext"/> class.
    /// </summary>
    /// <param name="realMatches">Real (hash-derived) matches between query and track.</param>
    /// <param name="queryProfile">Query spectral profile (caller guarantees non-null).</param>
    /// <param name="trackProfile">Track spectral profile (caller guarantees non-null).</param>
    /// <param name="queryLength">Query length in seconds.</param>
    /// <param name="trackLength">Track length in seconds.</param>
    /// <param name="fingerprintLength">Fingerprint length in seconds.</param>
    public SyntheticMatchContext(
        IReadOnlyList<MatchedWith> realMatches,
        SpectralProfile queryProfile,
        SpectralProfile trackProfile,
        double queryLength,
        double trackLength,
        double fingerprintLength)
    {
        RealMatches = realMatches;
        QueryProfile = queryProfile;
        TrackProfile = trackProfile;
        QueryLength = queryLength;
        TrackLength = trackLength;
        FingerprintLength = fingerprintLength;
    }

    /// <summary>
    ///  Gets the real (hash-derived) matches. Strategies use these as anchors for local bracket interpolation
    ///  in <see cref="SyntheticCandidateUtils"/>; synthetics from prior strategy calls are never fed back in.
    /// </summary>
    public IReadOnlyList<MatchedWith> RealMatches { get; }

    /// <summary>
    ///  Gets the query-side spectral profile. Never <c>null</c>.
    /// </summary>
    public SpectralProfile QueryProfile { get; }

    /// <summary>
    ///  Gets the track-side spectral profile. Never <c>null</c>.
    /// </summary>
    public SpectralProfile TrackProfile { get; }

    /// <summary>
    ///  Gets the query length in seconds.
    /// </summary>
    public double QueryLength { get; }

    /// <summary>
    ///  Gets the track length in seconds.
    /// </summary>
    public double TrackLength { get; }

    /// <summary>
    ///  Gets the fingerprint length in seconds.
    /// </summary>
    public double FingerprintLength { get; }
}