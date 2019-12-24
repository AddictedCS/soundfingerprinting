namespace SoundFingerprinting.Image
{
    using System.Collections.Generic;

    public class SSIM
    {
        public SSIM(byte[][] ssimImage, IEnumerable<Contour> contours)
        {
            SsimImage = ssimImage;
            Contours = contours;
        }
        
        public byte[][] SsimImage { get; }
        
        public IEnumerable<Contour> Contours { get; }
    }
}