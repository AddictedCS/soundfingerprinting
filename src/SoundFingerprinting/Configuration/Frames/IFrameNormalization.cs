namespace SoundFingerprinting.Configuration.Frames
{
    using System.Collections.Generic;
    using SoundFingerprinting.Data;

    public interface IFrameNormalization
    {
        /// <summary>
        ///  Normalizes frames
        /// </summary>
        /// <param name="frames">Frames to normalize</param>
        /// <returns>Normalized frames</returns>
        IEnumerable<Frame> Normalize(IEnumerable<Frame> frames);
    }
}