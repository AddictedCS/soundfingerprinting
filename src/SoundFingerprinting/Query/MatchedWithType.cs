namespace SoundFingerprinting.Query;

/// <summary>
///  Provenance of a <see cref="MatchedWith"/> entry in a coverage best-path: a real fingerprint hit, or which
///  spectral-bridging <see cref="SoundFingerprinting.SFM.ISfmMatchStrategy"/> synthesised it. Defaults to
///  <see cref="Fingerprint"/> so existing (pre-bridging) matches and deserialized data carry the real-match value.
/// </summary>
public enum MatchedWithType : byte
{
    /// <summary>
    ///  A real match derived from a fingerprint hash collision.
    /// </summary>
    Fingerprint = 0,

    /// <summary>
    ///  A synthetic bridge from <see cref="SoundFingerprinting.SFM.SilenceBridgingStrategy"/>.
    /// </summary>
    Silence = 1,

    /// <summary>
    ///  A synthetic bridge from <see cref="SoundFingerprinting.SFM.BroadbandNoiseBridgingStrategy"/>.
    /// </summary>
    BroadbandNoise = 2,

    /// <summary>
    ///  A synthetic bridge from <see cref="SoundFingerprinting.SFM.SimilarProfileBridgingStrategy"/>.
    /// </summary>
    SimilarProfile = 3,
}
