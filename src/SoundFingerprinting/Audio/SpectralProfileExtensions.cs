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
        public static SpectralProfile? GetSpectralProfile(this Hashes? hashes)
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

        /// <summary>
        ///  Returns a <see cref="TrackInfo"/> whose <see cref="TrackInfo.MetaFields"/> include well-known
        ///  SoundFingerprinting metadata sourced from <paramref name="hashes"/>'s <see cref="Hashes.Properties"/>.
        /// </summary>
        /// <remarks>
        ///  Well-known keys currently propagated: <see cref="SpectralProfileKeys.SpectralProfile"/>.
        ///  Existing entries in <paramref name="trackInfo"/>.MetaFields win on key collisions, so callers may
        ///  override propagated values explicitly. Returns the original <paramref name="trackInfo"/> instance
        ///  when no augmentation is needed.
        ///  <para>
        ///  Model service implementations call this at <c>Insert</c> time so query-side bridging
        ///  (<see cref="SoundFingerprinting.Configuration.QueryConfiguration.SfmMatchStrategy"/>) can locate the
        ///  spectral profile on <see cref="TrackInfo.MetaFields"/> without callers having to plumb it manually.
        ///  </para>
        /// </remarks>
        /// <param name="trackInfo">Track info to augment.</param>
        /// <param name="hashes">Hashes whose <see cref="Hashes.Properties"/> may carry well-known metadata. <c>null</c> is a no-op.</param>
        /// <returns>The same <paramref name="trackInfo"/> reference if nothing was propagated, otherwise a new copy with merged MetaFields.</returns>
        public static TrackInfo WithMetaFieldsFromHashes(this TrackInfo trackInfo, Hashes? hashes)
        {
            if (trackInfo == null || hashes == null)
            {
                return trackInfo!;
            }

            System.Collections.Generic.Dictionary<string, string>? merged = null;
            foreach (var key in WellKnownKeys)
            {
                if (trackInfo.MetaFields != null && trackInfo.MetaFields.ContainsKey(key))
                {
                    continue;
                }

                if (!hashes.Properties.TryGetValue(key, out var value))
                {
                    continue;
                }

                merged ??= trackInfo.MetaFields == null
                    ? new System.Collections.Generic.Dictionary<string, string>()
                    : new System.Collections.Generic.Dictionary<string, string>(trackInfo.MetaFields);
                merged[key] = value;
            }

            return merged == null
                ? trackInfo
                : new TrackInfo(trackInfo.Id, trackInfo.Title, trackInfo.Artist, merged, trackInfo.MediaType);
        }

        /// <summary>
        ///  Convenience overload that propagates well-known metadata from the audio side of <paramref name="hashes"/>.
        /// </summary>
        /// <remarks>
        ///  All currently-known propagatable keys are audio-only. If video-only keys are added later, this overload
        ///  should iterate both audio and video and merge in turn.
        /// </remarks>
        /// <param name="trackInfo">Track info to augment.</param>
        /// <param name="hashes">AV hashes container.</param>
        /// <returns>Augmented <see cref="TrackInfo"/>.</returns>
        public static TrackInfo WithMetaFieldsFromHashes(this TrackInfo trackInfo, AVHashes hashes)
        {
            return trackInfo.WithMetaFieldsFromHashes(hashes?.Audio);
        }

        /// <summary>
        ///  Returns <see cref="Hashes"/> whose <see cref="Hashes.Properties"/> include well-known SoundFingerprinting
        ///  metadata sourced from <paramref name="trackInfo"/>'s <see cref="TrackInfo.MetaFields"/>.
        /// </summary>
        /// <remarks>
        ///  Read-side mirror of <see cref="WithMetaFieldsFromHashes(TrackInfo, Hashes)"/>: model service implementations
        ///  call this when serving <c>ReadHashesByTrackId</c> so callers that query directly from the returned
        ///  <see cref="AVHashes"/> (without ever touching the <see cref="TrackInfo"/>) still get the spectral profile
        ///  on <c>Properties</c> and bridging works end-to-end.
        ///  Existing entries in <paramref name="hashes"/>.Properties win on key collisions.
        ///  Well-known keys currently propagated: <see cref="SpectralProfileKeys.SpectralProfile"/>.
        /// </remarks>
        /// <param name="hashes">Hashes to augment. <c>null</c> is a no-op.</param>
        /// <param name="trackInfo">Track info whose <see cref="TrackInfo.MetaFields"/> may carry well-known metadata.</param>
        /// <returns>The same <paramref name="hashes"/> reference if nothing was propagated, otherwise a new copy with merged Properties.</returns>
        public static Hashes? WithPropertiesFromTrack(this Hashes? hashes, TrackInfo trackInfo)
        {
            return hashes.WithPropertiesFromMetaFields(trackInfo?.MetaFields);
        }

        /// <summary>
        ///  Canonical read-side propagation: copy well-known SoundFingerprinting metadata from a generic
        ///  meta-fields dictionary into <paramref name="hashes"/>'s <see cref="Hashes.Properties"/>.
        /// </summary>
        /// <remarks>
        ///  Lower-level overload used when the caller already has a <see cref="System.Collections.Generic.IDictionary{TKey,TValue}"/>
        ///  (e.g. server-side storage paths reading a <c>TrackData.MetaFields</c> directly without materialising a
        ///  <see cref="TrackInfo"/>). Existing entries in <paramref name="hashes"/>.Properties win on key collisions.
        /// </remarks>
        /// <param name="hashes">Hashes to augment. <c>null</c> is a no-op.</param>
        /// <param name="metaFields">Meta-fields dictionary that may carry well-known keys. <c>null</c> is a no-op.</param>
        /// <returns>The same <paramref name="hashes"/> reference if nothing was propagated, otherwise the augmented hashes.</returns>
        public static Hashes? WithPropertiesFromMetaFields(this Hashes? hashes, System.Collections.Generic.IDictionary<string, string>? metaFields)
        {
            if (hashes == null || metaFields == null)
            {
                return hashes;
            }

            var augmented = hashes;
            foreach (var key in WellKnownKeys)
            {
                if (augmented.Properties.ContainsKey(key))
                {
                    continue;
                }

                if (!metaFields.TryGetValue(key, out var value))
                {
                    continue;
                }

                augmented = augmented.WithProperty(key, value);
            }

            return augmented;
        }

        private static readonly string[] WellKnownKeys =
        {
            SpectralProfileKeys.SpectralProfile,
        };

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
