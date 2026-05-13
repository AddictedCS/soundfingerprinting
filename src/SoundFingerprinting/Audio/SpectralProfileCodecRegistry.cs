namespace SoundFingerprinting.Audio
{
    using System;

    /// <summary>
    ///  Single-codec entry point for <see cref="SpectralProfile"/> encoding / decoding.
    /// </summary>
    /// <remarks>
    ///  v1 is the only supported wire format today. When a future version is needed, generalise to version dispatch
    ///  (the version byte at payload[0] already exists for exactly this purpose) rather than wedging branches in here.
    /// </remarks>
    public sealed class SpectralProfileCodecRegistry : ISpectralProfileCodecRegistry
    {
        /// <summary>
        ///  Well-known property key for the base64-encoded payload. Alias of <see cref="SpectralProfileKeys.SpectralProfile"/>.
        /// </summary>
        public const string SpectralProfileKey = SpectralProfileKeys.SpectralProfile;

        private readonly ISpectralProfileCodec codec = SpectralProfileCodecV1.Instance;

        private SpectralProfileCodecRegistry()
        {
        }

        /// <summary>
        ///  Gets the process-wide singleton. Non-null by construction.
        /// </summary>
        public static SpectralProfileCodecRegistry Default { get; } = new ();

        /// <inheritdoc />
        public SpectralProfile? Decode(string base64Payload)
        {
            if (string.IsNullOrEmpty(base64Payload))
            {
                return null;
            }

            byte[] payload;
            try
            {
                payload = Convert.FromBase64String(base64Payload);
            }
            catch (FormatException)
            {
                return null;
            }

            return Decode(payload);
        }

        /// <inheritdoc />
        public SpectralProfile? Decode(ReadOnlySpan<byte> payload)
        {
            if (payload.Length < 2 || payload[0] != codec.Version)
            {
                return null;
            }

            return codec.Decode(payload);
        }

        /// <inheritdoc />
        public string Encode(SpectralProfile profile)
        {
            return Convert.ToBase64String(codec.Encode(profile));
        }
    }
}
