namespace SoundFingerprinting.Audio
{
    using System.Collections.Generic;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Computes a per-second <see cref="SpectralProfile"/> from raw spectrogram frames and encodes it for transport.
    /// </summary>
    /// <remarks>
    ///  Must be invoked on the raw spectrum (before any frame normalization). Output is the base64-encoded payload
    ///  intended for <c>Hashes.Properties[SpectralProfileKeys.SpectralProfile]</c>.
    /// </remarks>
    public interface ISpectralProfileService
    {
        /// <summary>
        ///  Compute the base64-encoded spectral profile payload for the given pre-normalization frames.
        /// </summary>
        /// <param name="frames">Spectrogram frames (raw power, before normalization). Order does not matter — bucketing is by <see cref="Frame.StartsAt"/>.</param>
        /// <returns>Base64 payload, or <c>null</c> when <paramref name="frames"/> is empty.</returns>
        string? Encode(IReadOnlyList<Frame> frames);
    }
}
