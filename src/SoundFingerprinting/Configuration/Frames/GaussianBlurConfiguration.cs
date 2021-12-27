namespace SoundFingerprinting.Configuration.Frames
{
    /// <summary>
    ///  Gaussian blur configuration parameters.
    /// </summary>
    public class GaussianBlurConfiguration
    {
        /// <summary>
        ///  Gets an instance of default native gaussian filter that works best for fingerprints generated from images.
        /// </summary>
        public static GaussianBlurConfiguration Default => new (5, 1.5);

        /// <summary>
        /// Initializes a new instance of the <see cref="GaussianBlurConfiguration"/> class.
        /// </summary>
        /// <param name="kernel">Kernel size.</param>
        /// <param name="sigma">Gaussian sigma.</param>
        public GaussianBlurConfiguration(int kernel, double sigma)
        {
            Kernel = kernel;
            Sigma = sigma;
        }

        /// <summary>
        ///  Gets kernel size of the gaussian filter.
        /// </summary>
        public int Kernel { get; }

        /// <summary>
        ///  Gets sigma for gaussian filter.
        /// </summary>
        public double Sigma { get; }
    }
}