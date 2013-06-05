namespace Soundfingerprinting.Wavelets
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Soundfingerprinting.Infrastructure;

    public class WaveletService : IWaveletService
    {
        private readonly IWaveletDecomposition waveletDecomposition;

        public WaveletService()
            : this(DependencyResolver.Current.Get<IWaveletDecomposition>())
        {
        }

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