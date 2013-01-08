namespace Soundfingerprinting.SoundTools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;

    using Soundfingerprinting.Fingerprinting;
    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Fingerprinting.Wavelets;

    /// <summary>
    ///   Class for creating images from audio file
    /// </summary>
    /// <remarks>
    ///   Allows creation of the signal, spectrogram and wavelets images
    /// </remarks>
    public static class Imaging
    {
        /// <summary>
        /// Creates an image from top wavelet fingerprint data
        /// </summary>
        /// <param name="data">
        /// The concatenated fingerprint containing top wavelets
        /// </param>
        /// <param name="width">
        /// Width of the image
        /// </param>
        /// <param name="height">
        /// Height of the image
        /// </param>
        /// <returns>
        /// The System.Drawing.Image.
        /// </returns>
        public static Image GetFingerprintImage(bool[] data, int width, int height)
        {
            Bitmap image = new Bitmap(width, height, PixelFormat.Format16bppRgb565);

            // Scale the data and write to image
            for (int i = 0; i < width /*128*/; i++)
            {
                for (int j = 0; j < height - 1 /*32*/; j++)
                {
                    int color = data[(height * i) + (2 * j)] || data[(height * i) + (2 * j) + 1] ? 255 : 0;
                    image.SetPixel(i, j, Color.FromArgb(color, color, color));
                }
            }

            return image;
        }

        /// <summary>
        ///   Gets the full song representation of the fingerprints
        /// </summary>
        /// <param name = "data">Fingerprints</param>
        /// <param name = "width">Width of the image</param>
        /// <param name = "height">Height of the image</param>
        /// <returns>Bitmap representation of the fingerprints. All fingerprints in one file</returns>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1407:ArithmeticExpressionsMustDeclarePrecedence", Justification = "Reviewed. Suppression is OK here.")]
        public static Bitmap GetFingerprintsImage(List<bool[]> data, int width, int height)
        {
            const int ImagesPerRow = 5; /*5 bitmap images per line*/
            const int SpaceBetweenImages = 10; /*10 pixel space between images*/
            int fingersCount = data.Count;
            int rowCount = (int)Math.Ceiling((float)fingersCount / ImagesPerRow);

            int imageWidth = (ImagesPerRow * (width + SpaceBetweenImages)) + SpaceBetweenImages;
            int imageHeight = (rowCount * (height + SpaceBetweenImages)) + SpaceBetweenImages;

            Bitmap image = new Bitmap(imageWidth, imageHeight, PixelFormat.Format16bppRgb565);

            /*Change the background of the bitmap*/
            for (int i = 0; i < imageWidth; i++)
            {
                for (int j = 0; j < imageHeight; j++)
                {
                    image.SetPixel(i, j, Color.White);
                }
            }

            int verticalOffset = SpaceBetweenImages;
            int horizontalOffset = SpaceBetweenImages;
            int count = 0;
            for (int z = 0; z < data.Count; z++)
            {
                bool[] finger = data[z];
                for (int i = 0; i < width /*128*/; i++)
                {
                    for (int j = 0; j < height /*32*/; j++)
                    {
                        int color = finger[2 * height * i + 2 * j] || finger[2 * height * i + 2 * j + 1] ? 255 : 0;
                        image.SetPixel(i + horizontalOffset, j + verticalOffset, Color.FromArgb(color, color, color));
                    }
                }
                count++;
                if (count % ImagesPerRow == 0)
                {
                    verticalOffset += height + SpaceBetweenImages;
                    horizontalOffset = SpaceBetweenImages;
                }
                else
                {
                    horizontalOffset += width + SpaceBetweenImages;
                }
            }

            return image;
        }

        /// <summary>
        ///   Get image representation of the signal
        /// </summary>
        /// <param name = "data">Data to be drawn</param>
        /// <param name = "width">Width of the image</param>
        /// <param name = "height">Height of the image</param>
        /// <returns>Bitmap</returns>
        public static Bitmap GetSignalImage(float[] data, int width, int height)
        {
            Bitmap image = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(image);
            using (Brush brush = new SolidBrush(Color.Black))
            {
                graphics.FillRectangle(brush, new Rectangle(0, 0, width, height));
            }

            const int Gridline = 50; /*Every 50 pixels draw gridline*/
            /*Draw gridlines*/
            using (Pen pen = new Pen(Color.Red, 1))
            {
                /*Draw horizontal gridlines*/
                for (int i = 1; i < height / Gridline; i++)
                {
                    graphics.DrawLine(pen, 0, i * Gridline, width, i * Gridline);
                }

                /*Draw vertical gridlines*/
                for (int i = 1; i < width / Gridline; i++)
                {
                    graphics.DrawLine(pen, i * Gridline, 0, i * Gridline, height);
                }
            }

            int center = height / 2;
            /*Draw lines*/
            using (Pen pen = new Pen(Color.MediumSpringGreen, 1))
            {
                /*Find delta X, by which the lines will be drawn*/
                double deltaX = (double)width / data.Length;
                double normalizeFactor = data.Max((a) => Math.Abs(a)) / ((double)height / 2);
                for (int i = 0, n = data.Length; i < n; i++)
                {
                    graphics.DrawLine(
                        pen,
                        (float)(i * deltaX),
                        center,
                        (float)(i * deltaX),
                        (float)(center - (data[i] / normalizeFactor)));
                }
            }

            using (Pen pen = new Pen(Color.DarkGreen, 1))
            {
                /*Draw center line*/
                graphics.DrawLine(pen, 0, center, width, center);
            }

            FontFamily font = new FontFamily("Courier New");
            Font f = new Font(font, 10);
            Brush textbrush = Brushes.Orange;
            Point coordinate = new Point(10, 10);
            graphics.DrawString("https://code.google.com/p/soundfingerprinting/", f, textbrush, coordinate);
            return image;
        }

        /// <summary>
        /// Get a spectrogram of the signal specified at the input
        /// </summary>
        /// <param name="spectrum">
        /// The spectrum.
        /// </param>
        /// <param name="width">
        /// Width of the image
        /// </param>
        /// <param name="height">
        /// Height of the image
        /// </param>
        /// <returns>
        /// Spectral image of the signal
        /// </returns>
        /// <remarks>
        /// X axis - time
        ///   Y axis - frequency
        ///   Color - magnitude level of corresponding band value of the signal
        /// </remarks>
        public static Bitmap GetSpectrogramImage(float[][] spectrum, int width, int height)
        {
            Bitmap image = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(image);
            /*Fill Back color*/
            using (Brush brush = new SolidBrush(Color.Black))
            {
                graphics.FillRectangle(brush, new Rectangle(0, 0, width, height));
            }

            int bands = spectrum[0].Length;
            double max = spectrum.Max((b) => b.Max((v) => Math.Abs(v)));
            double deltaX = (double)(width - 1) / spectrum.Length; /*By how much the image will move to the left*/
            double deltaY = (double)(height - 1) / (bands + 1); /*By how much the image will move upward*/
            int prevX = 0;
            for (int i = 0, n = spectrum.Length; i < n; i++)
            {
                double x = i * deltaX;
                if ((int)x == prevX) continue;
                for (int j = 0, m = spectrum[0].Length; j < m; j++)
                {
                    Color color = ValueToBlackWhiteColor(spectrum[i][j], max / 10);
                    image.SetPixel((int)x, height - (int)(deltaY * j) - 1, color);
                }
                prevX = (int)x;
            }

            FontFamily font = new FontFamily("Courier New");
            Font f = new Font(font, 10);
            Brush textbrush = Brushes.Orange;
            Point coordinate = new Point(10, 10);
            graphics.DrawString("https://code.google.com/p/soundfingerprinting/", f, textbrush, coordinate);

            return image;
        }

        /// <summary>
        ///   Get corresponding grey pallet color of the spectrogram
        /// </summary>
        /// <param name = "value">Value</param>
        /// <param name = "maxValue">Max range of the values</param>
        /// <returns>Grey color corresponding to the value</returns>
        public static Color ValueToBlackWhiteColor(double value, double maxValue)
        {
            int color = (int)(Math.Abs(value) * 255 / Math.Abs(maxValue));
            if (color > 255)
                color = 255;
            return Color.FromArgb(color, color, color);
        }

        /// <summary>
        ///   Get color
        /// </summary>
        /// <param name = "mag">Magnitude [0; 1)</param>
        /// <returns>Associated color</returns>
        public static Color ValueToColor(double mag)
        {
            Color[] colors = new[]
                             {
                                 Color.FromArgb(0, 0, 0), Color.FromArgb(0, 0, 255), Color.FromArgb(0, 255, 255),
                                 Color.FromArgb(0, 255, 0), Color.FromArgb(255, 255, 0), Color.FromArgb(255, 0, 0),
                                 Color.FromArgb(255, 128, 128), Color.FromArgb(255, 255, 255)
                             };

            int numColors = colors.Length;

            double colorLoc = Math.Min(mag * numColors, numColors - 1);

            int lowCol = (int)(Math.Floor(colorLoc));
            int highCol = (int)(Math.Ceiling(colorLoc));
            double lowStrength = (highCol - colorLoc);
            double highStrength = 1.0 - lowStrength;

            int r = (int)(colors[lowCol].R * lowStrength +
                           colors[highCol].R * highStrength);
            int g = (int)(colors[lowCol].G * lowStrength +
                           colors[highCol].G * highStrength);
            int b = (int)(colors[lowCol].G * lowStrength +
                           colors[highCol].B * highStrength);
            Color magColor = Color.FromArgb(r, g, b);

            return magColor;
        }

        /// <summary>
        ///   Get wavelet decomposition on an image
        /// </summary>
        /// <param name = "wavelet">Wavelet used</param>
        /// <param name = "image">Image to be decomposed</param>
        /// <returns>New image</returns>
        public static Image GetWaveletTransformation(IWaveletDecomposition wavelet, float[][] image)
        {
            int width = image[0].Length;
            int height = image.Length;
            wavelet.DecomposeImageInPlace(image);
            Bitmap transformed = new Bitmap(width, height, PixelFormat.Format16bppRgb565);
            for (int i = 0; i < transformed.Height; i++)
                for (int j = 0; j < transformed.Width; j++)
                    transformed.SetPixel(j, i, Color.FromArgb((int)image[i][j]));

            return transformed;
        }


        /// <summary>
        ///   Gets the spectrum of the wavelet decomposition before extracting top wavelets and binary transformation
        /// </summary>
        /// <param name = "pathToFile">Path to file to be drawn</param>
        /// <param name = "stride">Stride within the fingerprint creation</param>
        /// <param name = "proxy">Proxy service</param>
        /// <param name = "service">Fingerprint service</param>
        /// <returns>Image to be saved</returns>
        public static Image GetWaveletSpectralImage(float[][] spectrum, IFingerprintingConfiguration configuration)
        {
            List<float[][]> wavelets = new List<float[][]>();
            int specLen = spectrum.GetLength(0);
            int start = configuration.Stride.GetFirstStride() / configuration.Overlap;
            int logbins = configuration.LogBins;
            int fingerprintLength = configuration.FingerprintLength;
            int overlap = configuration.Overlap;
            while (start + fingerprintLength < specLen)
            {
                float[][] frames = new float[fingerprintLength][];
                for (int i = 0; i < fingerprintLength; i++)
                {
                    frames[i] = new float[logbins];
                    Array.Copy(spectrum[start + i], frames[i], logbins);
                }

                start += fingerprintLength + (configuration.Stride.GetStride() / overlap);
                wavelets.Add(frames);
            }

            const int ImagesPerRow = 5; /*5 bitmap images per line*/
            const int SpaceBetweenImages = 10; /*10 pixel space between images*/
            int width = wavelets[0].GetLength(0);
            int height = wavelets[0][0].Length;
            int fingersCount = wavelets.Count;
            int rowCount = (int)Math.Ceiling((float)fingersCount / ImagesPerRow);

            int imageWidth = (ImagesPerRow * (width + SpaceBetweenImages)) + SpaceBetweenImages;
            int imageHeight = (rowCount * (height + SpaceBetweenImages)) + SpaceBetweenImages;

            Bitmap image = new Bitmap(imageWidth, imageHeight, PixelFormat.Format16bppRgb565);
            /*Change the background of the bitmap*/
            for (int i = 0; i < imageWidth; i++)
                for (int j = 0; j < imageHeight; j++)
                    image.SetPixel(i, j, Color.White);

            double maxValue = wavelets.Max((wavelet) => (wavelet.Max((column) => column.Max())));
            int verticalOffset = SpaceBetweenImages;
            int horizontalOffset = SpaceBetweenImages;
            int count = 0;
            double max = wavelets.Max(wav => wav.Max(w => w.Max(v => Math.Abs(v))));
            foreach (float[][] wavelet in wavelets)
            {
                for (int i = 0; i < width /*128*/; i++)
                {
                    for (int j = 0; j < height /*32*/; j++)
                    {
                        Color color = ValueToBlackWhiteColor(wavelet[i][j], max / 4);
                        image.SetPixel(i + horizontalOffset, j + verticalOffset, color);
                    }
                }
                count++;
                if (count % ImagesPerRow == 0)
                {
                    verticalOffset += height + SpaceBetweenImages;
                    horizontalOffset = SpaceBetweenImages;
                }
                else
                    horizontalOffset += width + SpaceBetweenImages;
            }
            return image;
        }
    }
}