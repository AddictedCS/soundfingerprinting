namespace SoundFingerprinting.Emy.AudioVideo.Config.Video
{
    public class BlackFramesFilterConfiguration
    {
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
