namespace SoundFingerprinting.Configuration.Frames
{
    public enum SsimImplementation
    {
        /// <summary>
        ///  None
        /// </summary>
        None = 0,
        
        /// <summary>
        ///  Structured similarity index
        /// </summary>
        SSIM = 1,
        
        /// <summary>
        /// Naive frame difference
        /// </summary>
        NaiveDiff = 3,
        
        /// <summary>
        ///  Gaussian filter with naive frame difference
        /// </summary>
        GaussianDiff = 4
    }
}
