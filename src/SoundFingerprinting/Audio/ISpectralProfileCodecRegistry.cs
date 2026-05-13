namespace SoundFingerprinting.Audio
{
    using System;

    /// <summary>
    ///  Registry of <see cref="ISpectralProfileCodec"/> implementations dispatched on the payload version byte.
    /// </summary>
    public interface ISpectralProfileCodecRegistry
    {
        /// <summary>
        ///  Decode a base64-encoded payload.
        /// </summary>
        /// <param name="base64Payload">Base64-encoded payload.</param>
        /// <returns>Decoded profile, or <c>null</c> if the version byte is unknown.</returns>
        SpectralProfile? Decode(string base64Payload);

        /// <summary>
        ///  Decode a raw payload (starts with version byte).
        /// </summary>
        /// <param name="payload">Raw bytes.</param>
        /// <returns>Decoded profile, or <c>null</c> if the version byte is unknown.</returns>
        SpectralProfile? Decode(ReadOnlySpan<byte> payload);

        /// <summary>
        ///  Encode a profile via the latest registered codec.
        /// </summary>
        /// <param name="profile">Profile to encode.</param>
        /// <returns>Base64-encoded payload.</returns>
        string Encode(SpectralProfile profile);
    }
}
