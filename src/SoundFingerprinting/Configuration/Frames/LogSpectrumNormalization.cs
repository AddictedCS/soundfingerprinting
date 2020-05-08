namespace SoundFingerprinting.Configuration.Frames
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Data;

    public class LogSpectrumNormalization : IFrameNormalization
    {
        public IEnumerable<Frame> Normalize(IEnumerable<Frame> frames)
        {
            return frames
                .AsParallel()
                .Select(frame =>
                {

                    float max = frame.ImageRowCols.Max(System.Math.Abs);
                    for (int i = 0; i < frame.ImageRowCols.Length; ++i)
                    {
                        float value = frame.ImageRowCols[i];
                        float scaled = System.Math.Min(value / max, 1);
                        int domain = 255;
                        float c = (float) (1 / System.Math.Log(1 + domain));
                        frame.ImageRowCols[i] = (float) (c * System.Math.Log(1 + scaled * domain));
                    }

                    return frame;
                });
        }
    }
}