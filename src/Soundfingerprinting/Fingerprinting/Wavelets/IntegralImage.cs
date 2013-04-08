namespace Soundfingerprinting.Fingerprinting.Wavelets
{
    public class IntegralImage : IWaveletDecomposition
    {
        #region IWaveletDecomposition Members

        public void DecomposeImageInPlace(float[][] frames)
        {
            int nframes = frames.Length;
            for (int y = 1; y < nframes; y++)
            {
                frames[y][0] += frames[y - 1][0];
            }
            int nbands = frames[0].Length;
            for (int x = 1; x < nbands; x++)
            {
                frames[0][x] += frames[0][x - 1];
            }

            for (int y = 1; y < nframes; y++)
            {
                for (int x = 1; x < nbands; x++)
                {
                    frames[y][x] += frames[y - 1][x] + frames[y][x - 1] - frames[y - 1][x - 1];
                }
            }
        }

        #endregion
    }
}