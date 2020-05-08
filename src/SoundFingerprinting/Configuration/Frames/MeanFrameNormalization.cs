namespace SoundFingerprinting.Configuration.Frames
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Data;

    public class MeanFrameNormalization : IFrameNormalization
    {
        public IEnumerable<Frame> Normalize(IEnumerable<Frame> frames)
        {
            return frames
                .AsParallel()
                .Select(frame =>
                {
                    float avg = frame.ImageRowCols.Average(_ => _);
                    for (int i = 0; i < frame.ImageRowCols.Length; ++i)
                    {
                        float value = frame.ImageRowCols[i];
                        float scaled = value - avg;
                        frame.ImageRowCols[i] = scaled;
                    }

                    return frame;
                });
        }
    }
}