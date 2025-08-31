namespace SoundFingerprinting.Configuration.Frames
{
    /// <summary>
    ///  Class that defines black frame filter properties.
    /// </summary>
    public class BlackFramesFilterConfiguration
    {
        /// <summary>
        ///  Gets the default configuration that considers a frame black if 94% of its pixels are below the threshold of 32.
        /// </summary>
        public static readonly BlackFramesFilterConfiguration Default = new ()
        {
            Amount = 94, 
            Threshold = 32 
        };
        
        /// <summary>
        /// Gets or sets the threshold below which a pixel value is considered black.
        /// </summary>
        public byte Threshold { get; set; }

        /// <summary>
        /// Gets or sets the minimum percentage of the pixels in a frame that have to be below the <see cref="Threshold" />
        /// so that it is considered a black frame.
        /// </summary>
        public float Amount { get; set; }
    }
}
