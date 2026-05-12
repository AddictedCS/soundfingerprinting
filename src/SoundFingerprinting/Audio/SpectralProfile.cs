namespace SoundFingerprinting.Audio
{
    using System.Collections.Generic;
    using SoundFingerprinting.SFM;

    /// <summary>
    ///  Per-second spectral character of an audio track.
    /// </summary>
    /// <remarks>
    ///  Captured pre-normalization during fingerprint extraction when
    ///  <see cref="SoundFingerprinting.Configuration.FingerprintConfiguration.ComputeSpectralProfile"/>
    ///  is enabled. Persisted as a base64 payload under <see cref="SpectralProfileKeys.SpectralProfile"/>
    ///  in <c>TrackInfo.MetaFields</c> (track side) and <c>Hashes.Properties</c> (query side).
    ///  Consumed by <see cref="ISfmMatchStrategy"/> to synthesise per-second
    ///  bridge candidates that augment the LIS input prior to path reconstruction.
    /// </remarks>
    public sealed class SpectralProfile
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="SpectralProfile"/> class.
        /// </summary>
        /// <param name="perSecond">Per-second metrics, indexed by integer second from <c>0</c> to <c>length-1</c>.</param>
        public SpectralProfile(IReadOnlyList<SpectralSecond> perSecond)
        {
            PerSecond = perSecond;
        }

        /// <summary>
        ///  Gets per-second SFM and power metrics.
        /// </summary>
        public IReadOnlyList<SpectralSecond> PerSecond { get; }

        /// <summary>
        ///  Gets the number of seconds covered by this profile.
        /// </summary>
        public int LengthInSeconds => PerSecond.Count;
    }
}
