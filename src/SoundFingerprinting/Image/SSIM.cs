namespace SoundFingerprinting.Image
{
    using System.Collections.Generic;

    public class SSIM
    {
        public SSIM(byte[][] ssimImage, byte[][] thresholdInvertedImage, IEnumerable<Contour> contours)
        {
            SsimImage = ssimImage;
            ThresholdInvertedImage = thresholdInvertedImage;
            Contours = contours;
        }

        public byte[][] SsimImage { get; }

        public byte[][] ThresholdInvertedImage { get; }
        
        public IEnumerable<Contour> Contours { get; }
    }
}