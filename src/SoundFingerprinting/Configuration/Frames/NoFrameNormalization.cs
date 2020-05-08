namespace SoundFingerprinting.Configuration.Frames
{
    using System.Collections.Generic;
    using SoundFingerprinting.Data;

    public class NoFrameNormalization : IFrameNormalization
    {
        public IEnumerable<Frame> Normalize(IEnumerable<Frame> frames)
        {
            return frames;
        }
    }
}