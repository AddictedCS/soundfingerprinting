namespace SoundFingerprinting.Wavelets
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class IntegralImage : WaveletDecomposition
    {
        public override void DecomposeImagesInPlace(IEnumerable<float[][]> images)
        {
            Parallel.ForEach(images, DecomposeImageInPlace);
        }

        public override void DecomposeImageInPlace(float[][] image)
        {
            int nframes = image.Length;
            for (int y = 1; y < nframes; y++)
            {
                image[y][0] += image[y - 1][0];
            }

            int nbands = image[0].Length;
            for (int x = 1; x < nbands; x++)
            {
                image[0][x] += image[0][x - 1];
            }

            for (int y = 1; y < nframes; y++)
            {
                for (int x = 1; x < nbands; x++)
                {
                    image[y][x] += image[y - 1][x] + image[y][x - 1] - image[y - 1][x - 1];
                }
            }
        }
    }
}