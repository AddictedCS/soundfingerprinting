namespace Soundfingerprinting.Fingerprinting.Wavelets
{
    using System.Collections.Generic;

    public class WaveletService : IWaveletService
    {
        private readonly IWaveletDecomposition waveletDecomposition;

        public WaveletService(IWaveletDecomposition waveletDecomposition)
        {
            this.waveletDecomposition = waveletDecomposition;
        }

        public void ApplyWaveletTransformInPlace(List<float[][]> logarithmizedSpectrum)
        {
            foreach (var image in logarithmizedSpectrum)
            {
                this.waveletDecomposition.DecomposeImageInPlace(image); /*Compute wavelets*/
            }
        }
    }
}