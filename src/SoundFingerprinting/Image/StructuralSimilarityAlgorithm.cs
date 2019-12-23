namespace SoundFingerprinting.Image
{
    using System;
    using System.Collections.Generic;

    public class StructuralSimilarityAlgorithm
    {
        private readonly double[,] kernel2d = GaussianBlurKernel.Kernel2D(11, 1.5);

        /// <summary>
        ///  Returns contours that are relevant across two input images
        /// </summary>
        /// <param name="img1">First image</param>
        /// <param name="img2">Second image</param>
        /// <param name="differenceThreshold">How much difference is considered relevant before we consider a pixel as a contour element (value between 0,255)</param>
        /// <param name="areaThreshold">How big should be the area of the contour, before it is considered relevant (min value 2)</param>
        /// <returns>List of contours</returns>
        public IEnumerable<Contour> FindContours(GrayImage img1, GrayImage img2, int differenceThreshold, int areaThreshold)
        {
            if(img1.Width != img2.Width)
                throw new ArgumentException(nameof(img1.Width));
            if(differenceThreshold < 0 || differenceThreshold > 255)
                throw new AggregateException(nameof(differenceThreshold));
            if(areaThreshold < 2)
                throw new ArgumentException(nameof(areaThreshold));
                
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

            var k1 = 0.01;
            var k2 = 0.03;
            var L = 255;

            var C1 = Math.Pow(k1 * L, 2);
            var C2 = Math.Pow(k2 * L, 2);
            var A1 = uxUy.Convert(x => (float)(2 * x + C1));                       //2 * ux * uy + C1,
            var A2 = vxy.Convert(x => (float)(2 * x + C2));                        //2 * vxy + C2
            var B1 = uxSquared.Add(uySquared).Convert(x => (float)(x + C1));       //ux ** 2 + uy ** 2 + C1
            var B2 = vx.Add(vy).Convert(x => (float)(x + C2));                     //vx + vy + C2)  
            var D = B1.Multiply(B2);
            var S = A1.Multiply(A2).Divide(D);

            byte[][] ssim = S.ConvertAndUnwrap(x => (byte)(x * byte.MaxValue));
            ImageService.Instance.ThresholdInvInPlace(ssim, differenceThreshold, byte.MaxValue);
            return Contour.FindContours(ssim, byte.MaxValue, areaThreshold);
        }
    }
}