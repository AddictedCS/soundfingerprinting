namespace SoundFingerprinting.Configuration.Frames;

using System.Collections.Generic;
using System.Linq;
using SoundFingerprinting.Data;
using SoundFingerprinting.Image;

/// <summary>
///  Gaussian blur frame normalization technique.
/// </summary>
public class GaussianBlurFrameNormalization : IFrameNormalization
{
    private readonly double[,] kernel;

    /// <summary>
    /// Initializes a new instance of the <see cref="GaussianBlurFrameNormalization"/> class.
    /// </summary>
    /// <param name="configuration">Gaussian blur configuration.</param>
    public GaussianBlurFrameNormalization(GaussianBlurConfiguration configuration)
    {
        kernel = GaussianBlurKernel.Kernel2D(configuration.Kernel, configuration.Sigma);
    }

    /// <inheritdoc cref="IFrameNormalization.Normalize"/>
    public IEnumerable<Frame> Normalize(IEnumerable<Frame> frames)
    {
        return BlurFrames(frames);
    }
    
    private IEnumerable<Frame> BlurFrames(IEnumerable<Frame> frames)
    {
        return frames
            .AsParallel()
            .Select(frame =>
            {
                float[][] image = ImageService.RowCols2Image(frame.ImageRowCols, frame.Rows, frame.Cols);
                float[][] blurred = GrayImage.Convolve(image, kernel);
                return new Frame(blurred, frame.StartsAt, frame.SequenceNumber);
            })
            .ToList();
    }
}