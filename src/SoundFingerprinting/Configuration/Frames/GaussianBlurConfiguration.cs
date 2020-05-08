namespace SoundFingerprinting.Configuration.Frames
{
    using System;

    public class GaussianBlurConfiguration
    {
        /// <summary>
        ///  Gets an instance of configuration that will apply no gaussian filter
        /// </summary>
        public static GaussianBlurConfiguration None => new GaussianBlurConfiguration();

        /// <summary>
        ///  Gets an instance of default native gaussian filter that works best for fingerprints generated from images
        /// </summary>
        public static GaussianBlurConfiguration Default => new GaussianBlurConfiguration(5, 1.5, GaussianFilter.Default);

        private GaussianBlurConfiguration()
        {
            GaussianFilter = GaussianFilter.None;
        }

        public GaussianBlurConfiguration(int kernel, double sigma, GaussianFilter filterType)
        {
            if (filterType == GaussianFilter.None)
            {
                throw new ArgumentException($"Can't combine None configuration with kernel {kernel} and sigma {sigma}");
            }

            Kernel = kernel;
            Sigma = sigma;
            GaussianFilter = filterType;
        }

        /// <summary>
        ///  Gets type of the gaussian filter
        /// </summary>
        public GaussianFilter GaussianFilter { get; }

        /// <summary>
        ///  Gets kernel size of the gaussian filter
        /// </summary>
        public int Kernel { get; }

        /// <summary>
        ///  Gets sigma for gaussian filter
        /// </summary>
        public double Sigma { get; }
    }
}