namespace SoundFingerprinting.Audio
{
    using System;

    /// <summary>
    ///  Encodes and decodes a single version of the <see cref="SpectralProfile"/> wire format.
    /// </summary>
    /// <remarks>
    ///  The first byte of every encoded payload is a version byte read by
    ///  <see cref="ISpectralProfileCodecRegistry"/> to dispatch to the correct codec.
    /// </remarks>
    public interface ISpectralProfileCodec
    {
        /// <summary>
        ///  Gets the version byte this codec handles. Matches byte 0 of every encoded payload it produces.
        /// </summary>
        byte Version { get; }

        /// <summary>
        ///  Decode a payload whose version byte already matched <see cref="Version"/>.
        /// </summary>
        /// <param name="payload">Raw bytes including the version byte at index 0 and flags byte at index 1.</param>
        /// <returns>Decoded profile.</returns>
        SpectralProfile Decode(ReadOnlySpan<byte> payload);

        /// <summary>
        ///  Encode a profile as version + flags + data.
        /// </summary>
        /// <param name="profile">Profile to encode.</param>
        /// <returns>Raw byte array starting with <see cref="Version"/> at index 0.</returns>
        byte[] Encode(SpectralProfile profile);
    }
}
