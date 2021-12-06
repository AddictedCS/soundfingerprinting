namespace SoundFingerprinting.Configuration.Frames
{
    /// <summary>
    ///  Class that defines cropping configuration.
    /// </summary>
    public class CroppingConfiguration
    {
        /// <summary>
        ///  Gets or sets crop detector to use that will be used to identify region to crop.
        /// </summary>
        public CropDetector Detector { get; set; }

        /// <summary>
        ///  Gets or sets additional crop detector parameters.
        /// </summary>
        public string Parameters { get; set; } = null!;
    }
}
