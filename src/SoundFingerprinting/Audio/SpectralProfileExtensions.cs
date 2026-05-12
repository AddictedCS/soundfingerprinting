namespace SoundFingerprinting.Audio
{
    using System;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Read/write the spectral profile property on <see cref="TrackInfo.MetaFields"/> and <see cref="Hashes.Properties"/>.
    /// </summary>
    public static class SpectralProfileExtensions
    {
        /// <summary>
        ///  Try to decode the spectral profile attached to <paramref name="trackInfo"/>'s metadata.
        /// </summary>
        /// <param name="trackInfo">Track info.</param>
        /// <returns>Decoded profile, or <c>null</c> when absent, malformed, or version unknown.</returns>
        public static SpectralProfile? GetSpectralProfile(this TrackInfo trackInfo)
        {
            if (trackInfo?.MetaFields == null)
            {
                return null;
            }

            return trackInfo.MetaFields.TryGetValue(SpectralProfileKeys.SpectralProfile, out var base64) ? DecodeViaRegistry(base64) : null;
        }

        /// <summary>
        ///  Attach a spectral profile to <paramref name="trackInfo"/>'s metadata.
        /// </summary>
        /// <param name="trackInfo">Track info.</param>
        /// <param name="profile">Profile to attach. <c>null</c> removes any existing entry.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="trackInfo"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        ///  Thrown when <c>trackInfo.MetaFields</c> is <c>null</c> — silently dropping the write would mask
        ///  persistence failures on the backfill path (which calls <c>IModelService.UpdateTrack</c> after Set).
        /// </exception>
        public static void SetSpectralProfile(this TrackInfo trackInfo, SpectralProfile? profile)
        {
            if (trackInfo == null)
            {
                throw new ArgumentNullException(nameof(trackInfo));
            }

            if (trackInfo.MetaFields == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TrackInfo)}.{nameof(TrackInfo.MetaFields)} is null; cannot attach a spectral profile. " +
                    $"Construct {nameof(TrackInfo)} via a ctor that initialises MetaFields, or assign a dictionary before calling SetSpectralProfile.");
            }

            if (profile == null)
            {
                trackInfo.MetaFields.Remove(SpectralProfileKeys.SpectralProfile);
                return;
            }

            trackInfo.MetaFields[SpectralProfileKeys.SpectralProfile] = EncodeViaRegistry(profile);
        }

        /// <summary>
        ///  Try to decode the spectral profile attached to <paramref name="hashes"/>'s properties.
        /// </summary>
        /// <param name="hashes">Hashes.</param>
        /// <returns>Decoded profile, or <c>null</c> when absent, malformed, or version unknown.</returns>
        public static SpectralProfile? GetSpectralProfile(this Hashes hashes)
        {
            if (hashes == null)
            {
                return null;
            }

            return hashes.Properties.TryGetValue(SpectralProfileKeys.SpectralProfile, out var base64) ? DecodeViaRegistry(base64) : null;
        }

        /// <summary>
        ///  Attach a spectral profile to <paramref name="hashes"/>'s properties.
        /// </summary>
        /// <param name="hashes">Hashes.</param>
        /// <param name="profile">Profile to attach. Must not be <c>null</c>.</param>
        /// <returns>New <see cref="Hashes"/> with the profile attached.</returns>
        public static Hashes WithSpectralProfile(this Hashes hashes, SpectralProfile profile)
        {
            return hashes.WithProperty(SpectralProfileKeys.SpectralProfile, EncodeViaRegistry(profile));
        }

        private static SpectralProfile? DecodeViaRegistry(string base64)
        {
            return SpectralProfileCodecRegistry.Default.Decode(base64);
        }

        private static string EncodeViaRegistry(SpectralProfile profile)
        {
            return SpectralProfileCodecRegistry.Default.Encode(profile);
        }
    }
}
