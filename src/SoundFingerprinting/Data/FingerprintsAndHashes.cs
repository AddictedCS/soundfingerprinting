namespace SoundFingerprinting.Data;

using System.Collections.Generic;

/// <summary>
///  Class containing original fingerprints and hashes associated with them.
/// </summary>
public class FingerprintsAndHashes
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FingerprintsAndHashes"/> class.
    /// </summary>
    /// <param name="fingerprints">Fingerprints.</param>
    /// <param name="hashes">Hashes.</param>
    public FingerprintsAndHashes(IEnumerable<Fingerprint> fingerprints, Hashes hashes)
    {
        Fingerprints = fingerprints;
        Hashes = hashes;
    }

    /// <summary>
    ///  Gets original fingerprints.
    /// </summary>
    public IEnumerable<Fingerprint> Fingerprints { get; }

    /// <summary>
    ///  Gets hashes.
    /// </summary>
    public Hashes Hashes { get; }

    /// <summary>
    ///  Deconstructs fingerprints and hashes object.
    /// </summary>
    /// <param name="fingerprints">Fingerprints.</param>
    /// <param name="hashes">Hashes.</param>
    public void Deconstruct(out IEnumerable<Fingerprint> fingerprints, out Hashes hashes)
    {
        fingerprints = Fingerprints;
        hashes = Hashes;
    }
}