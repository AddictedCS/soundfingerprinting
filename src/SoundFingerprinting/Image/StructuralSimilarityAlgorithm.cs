namespace SoundFingerprinting.Image
{
    using System;
    using System.Linq;

    public class StructuralSimilarityAlgorithm
    {
        private const double K1 = 0.01;
        private const double K2 = 0.03;
        private const int L = 255;

        /// <summary>
        ///  Returns contours that are relevant across two input images
        /// </summary>
        /// <param name="img1">First image</param>
        /// <param name="img2">Second image</param>
        /// <param name="differenceThreshold">How much difference is considered relevant before we consider a pixel as a contour element (value between 0,255)</param>
        /// <param name="areaThreshold">How big should be the area of the contour, before it is considered relevant (min value 2)</param>
        /// <param name="kernelSize">Gaussian blur kernel size (i.e. 11)</param>
        /// <param name="sigma">Gaussian kernel sigma over X and Y directions (i.e. 1.5)</param>
        /// <param name="borderWidth">Allowed border width (i.e. 5)</param>
        /// <returns>List of contours, thresholded and SSIM image</returns>
        public SSIM FindContours(GrayImage img1, GrayImage img2,
            int differenceThreshold,
            int areaThreshold,
            int kernelSize,
            double sigma,
            int borderWidth)
        {
            if (img1.Width != img2.Width)
                throw new ArgumentException(nameof(img1.Width));
            if (differenceThreshold < 0 || differenceThreshold > 255)
                throw new AggregateException(nameof(differenceThreshold));
            if (areaThreshold < 2)
                throw new ArgumentException(nameof(areaThreshold));

            double[,] kernel2d = GaussianBlurKernel.Kernel2D(kernelSize, sigma);
            var ux = img1.GaussianBlur(kernel2d);
            var uy = img2.GaussianBlur(kernel2d);
            var uxx = img1.Multiply(img1).GaussianBlur(kernel2d);
            var uyy = img2.Multiply(img2).GaussianBlur(kernel2d);
            var uxy = img1.Multiply(img2).GaussianBlur(kernel2d);

            var uxSquared = ux.Multiply(ux);
            var vx = uxx.Subtract(uxSquared);
            var uySquared = uy.Multiply(uy);
            var vy = uyy.Subtract(uySquared);
            var uxUy = ux.Multiply(uy);
            var vxy = uxy.Subtract(uxUy);


            double c1 = Math.Pow(K1 * L, 2);
            double c2 = Math.Pow(K2 * L, 2);
            var a1 = uxUy.Convert(x => (float) (2 * x + c1));                     //2 * ux * uy + C1,
            var a2 = vxy.Convert(x => (float) (2 * x + c2));                      //2 * vxy + C2
            var b1 = uxSquared.Add(uySquared).Convert(x => (float) (x + c1));     //ux ** 2 + uy ** 2 + C1
            var b2 = vx.Add(vy).Convert(x => (float) (x + c2));                   //vx + vy + C2)  
            var d = b1.Multiply(b2);
            var s = a1.Multiply(a2).Divide(d);
            byte[][] ssimImage = s.ConvertAndUnwrap(x => (byte) (x * byte.MaxValue));
            byte[][] thresholdInvImage = ssimImage
                .Skip(borderWidth)
                .Take(img1.Height - 2 * borderWidth)
                .Select(x => x.Skip(borderWidth).Take(img1.Width - 2 * borderWidth).ToArray())
                .ToArray();

            ThresholdInvInPlace(thresholdInvImage, differenceThreshold, byte.MaxValue);
            return new SSIM(ssimImage, thresholdInvImage, Contour.FindContours(thresholdInvImage, byte.MaxValue, areaThreshold));
        }

        private static void ThresholdInvInPlace(byte[][] image, int threshold, byte maxValue)
        {
            foreach (byte[] row in image)
            {
                for (int j = 0; j < row.Length; ++j)
                {
                    row[j] = row[j] > threshold ? byte.MinValue : maxValue;
                }
            }
        }
    }
}