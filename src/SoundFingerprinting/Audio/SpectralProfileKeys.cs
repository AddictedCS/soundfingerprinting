namespace SoundFingerprinting.Audio
{
    /// <summary>
    ///  Well-known property keys for spectral profile transport.
    /// </summary>
    public static class SpectralProfileKeys
    {
        /// <summary>
        ///  Property key used to store the base64-encoded profile payload on both
        ///  <c>TrackInfo.MetaFields</c> and <c>Hashes.Properties</c>.
        /// </summary>
        public const string SpectralProfile = "spectralProfile";
    }
}
