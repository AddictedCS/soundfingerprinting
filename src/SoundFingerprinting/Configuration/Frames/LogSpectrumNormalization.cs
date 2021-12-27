namespace SoundFingerprinting.Configuration.Frames
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Logarithm image normalization.
    /// </summary>
    /// <remarks>
    ///  This has the effect that low intensity pixel values are enhanced. <br />
    ///  Applied on the audio spectrogram, allows enhancing small frequency peaks which would have been disregarded by min-hash hashing algorithm. <br />
    ///  Theoretical details can be found <a href="https://homepages.inf.ed.ac.uk/rbf/HIPR2/pixlog.htm">here</a>.
    /// </remarks>
    public class LogSpectrumNormalization : IFrameNormalization
    {
        private const int Domain = 255;
        
        /// <summary>
        ///  Normalizes input frames.
        /// </summary>
        /// <param name="frames">Frames to normalize.</param>
        /// <returns>List of normalized image frames.</returns>
        public IEnumerable<Frame> Normalize(IEnumerable<Frame> frames)
        {
            return frames
                .AsParallel()
                .Select(frame =>
                {
                    float c = (float)(1 / System.Math.Log(1 + Domain));
                    float max = frame.ImageRowCols.Max(System.Math.Abs);
                    for (int i = 0; i < frame.ImageRowCols.Length; ++i)
                    {
                        float value = frame.ImageRowCols[i];
                        float scaled = System.Math.Min(value / max, 1);
                        frame.ImageRowCols[i] = (float)(c * System.Math.Log(1 + scaled * Domain));
                    }

                    return frame;
                });
        }
    }
}