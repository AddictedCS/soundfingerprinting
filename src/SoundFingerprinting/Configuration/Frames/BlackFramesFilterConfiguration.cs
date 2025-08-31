namespace SoundFingerprinting.Configuration.Frames
{
    /// <summary>
    ///  Class that defines black frame filter properties.
    /// </summary>
    public class BlackFramesFilterConfiguration(byte threshold, float amount)
    {
        /// <summary>
        ///  Gets the default configuration that considers a frame black if 94% of its pixels are below the threshold of 32.
        /// </summary>
        /// <remarks>
        ///  A good default for edge search strategy.
        /// </remarks>
        public static readonly BlackFramesFilterConfiguration EdgeSearchDefault = new (32, 94f);
        
        /// <summary>
        ///  Gets the default configuration that considers a frame black if 98% of its pixels are below the threshold of 32.
        /// </summary>
        /// <remarks>
        ///  A good default for video fingerprinting.
        /// </remarks>
        public static readonly BlackFramesFilterConfiguration FingerprintingDefault = new (32, 98f);
        
        /// <summary>
        /// Gets the threshold below which a pixel value is considered black.
        /// </summary>
        public byte Threshold { get; } = threshold;

        /// <summary>
        /// Gets the minimum percentage of the pixels in a frame that have to be below the <see cref="Threshold" />
        /// so that it is considered a black frame.
        /// </summary>
        public float Amount { get; } = amount;
    }
}
