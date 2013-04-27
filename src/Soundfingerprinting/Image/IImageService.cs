namespace Soundfingerprinting.Image
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;

    //public interface IImageService
    //{

    //}

    //public class ImageService : IImageService
    //{
    //    private const int SpaceBetweenImages = 10;      /*10 pixel space between fingerprint images*/

    //    public Image GetImageForFingerprint(bool[] data, int width, int height)
    //    {
    //        Bitmap image = new Bitmap(width, height, PixelFormat.Format16bppRgb565);
    //        DrawFingerprintInImage(image, data, width, height, 0, 0);
    //        return image;
    //    }

    //    public Image GetImageForFingerprints(List<bool[]> fingerprints, int width, int height, int fingerprintsPerRow)
    //    {
    //        int imagesPerRow = fingerprintsPerRow; /*5 bitmap images per line*/
    //        int fingersCount = fingerprints.Count;
    //        int rowCount = (int)Math.Ceiling((float)fingersCount / imagesPerRow);
    //        int imageWidth = (imagesPerRow * (width + SpaceBetweenImages)) + SpaceBetweenImages;
    //        int imageHeight = (rowCount * (height + SpaceBetweenImages)) + SpaceBetweenImages;

    //        Bitmap image = new Bitmap(imageWidth, imageHeight, PixelFormat.Format16bppRgb565);
    //        SetBackground(image, Color.White);

    //        int verticalOffset = SpaceBetweenImages;
    //        int horizontalOffset = SpaceBetweenImages;
    //        int count = 0;
    //        foreach (bool[] fingerprint in fingerprints)
    //        {
    //            DrawFingerprintInImage(image, fingerprint, width, height, horizontalOffset, verticalOffset);
    //            count++;
    //            if (count % imagesPerRow == 0)
    //            {
    //                verticalOffset += height + SpaceBetweenImages;
    //                horizontalOffset = SpaceBetweenImages;
    //            }
    //            else
    //            {
    //                horizontalOffset += width + SpaceBetweenImages;
    //            }
    //        }

    //        return image;
    //    }

    //    public Image GetWaveletsImageFromSpetrum()

    //    private void DrawFingerprintInImage(
    //        Bitmap image, bool[] fingerprint, int fingerprintWidth, int fingerprintHeight, int xOffset, int yOffset)
    //    {
    //        // Scale the fingerprints and write to image
    //        for (int i = 0; i < fingerprintWidth /*128*/; i++)
    //        {
    //            for (int j = 0; j < fingerprintHeight /*32*/; j++)
    //            {
    //                //if 10 or 01 element then its white
    //                Color color = fingerprint[(fingerprintHeight * i) + (2 * j)]
    //                              || fingerprint[(fingerprintHeight * i) + (2 * j) + 1]
    //                                  ? Color.White
    //                                  : Color.Black;
    //                image.SetPixel(xOffset + i, yOffset + j, color);
    //            }
    //        }
    //    }

    //    private void SetBackground(Bitmap image, Color color)
    //    {
    //        for (int i = 0; i < image.Width; i++)
    //        {
    //            for (int j = 0; j < image.Height; j++)
    //            {
    //                image.SetPixel(i, j, color);
    //            }
    //        }
    //    }
    //}
}
