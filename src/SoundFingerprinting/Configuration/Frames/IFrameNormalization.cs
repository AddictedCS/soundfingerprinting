namespace SoundFingerprinting.Configuration.Frames
{
    using System.Collections.Generic;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Frame normalization strategy.
    /// </summary>
    public interface IFrameNormalization
    {
        /// <summary>
        ///  Normalizes output frames.
        /// </summary>
        /// <param name="frames">Frames to normalize.</param>
        /// <returns>Normalized frames.</returns>
        /// <remarks>
        ///  For audio fingerprints frames represent log spectrogram images. <br/>
        ///  For video fingerprints frames represent actual video frame images.
        /// </remarks>
        IEnumerable<Frame> Normalize(IEnumerable<Frame> frames);
    }
}