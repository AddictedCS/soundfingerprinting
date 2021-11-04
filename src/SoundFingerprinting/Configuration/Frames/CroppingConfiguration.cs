namespace SoundFingerprinting.Configuration.Frames
{
    public class CroppingConfiguration
    {
        public CropDetector Detector { get; set; }

        public string Parameters { get; set; } = null!;
    }
}
