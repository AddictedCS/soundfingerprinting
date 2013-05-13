namespace Soundfingerprinting.Fingerprinting.Wavelets
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class WaveletService : IWaveletService
    {
        private readonly IWaveletDecomposition waveletDecomposition;

        public WaveletService(IWaveletDecomposition waveletDecomposition)
        {
            this.waveletDecomposition = waveletDecomposition;
        }

        public void ApplyWaveletTransformInPlace(List<float[][]> logarithmizedSpectrum)
        {
            Parallel.ForEach(
                logarithmizedSpectrum,
                image => waveletDecomposition.DecomposeImageInPlace(image));
        }
    }
}