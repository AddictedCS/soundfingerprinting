namespace SoundFingerprinting.Data;

using System.Collections.Generic;

/// <summary>
///  Class containing audio/video fingerprints, before those are getting hashed.
/// </summary>
public class AVFingerprints
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AVFingerprints"/> class.
    /// </summary>
    /// <param name="audio">Audio fingerprints</param>
    /// <param name="video">Video fingerprints</param>
    public AVFingerprints(IEnumerable<Fingerprint> audio, IEnumerable<Fingerprint> video)
    {
        Audio = audio;
        Video = video;
    }

    /// <summary>
    ///  Gets audio fingerprints.
    /// </summary>
    public IEnumerable<Fingerprint> Audio { get; }

    /// <summary>
    ///  Gets video fingerprints.
    /// </summary>
    public IEnumerable<Fingerprint> Video { get; }
}