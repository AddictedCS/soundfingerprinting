namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Strides;
    using SoundFingerprinting.Utils;
    using SoundFingerprinting.Wavelets;

    public class FingerprintService : IFingerprintService
    {
        private readonly ISpectrumService spectrumService;

        private readonly IWaveletService waveletService;

        private readonly IFingerprintDescriptor fingerprintDescriptor;

        public FingerprintService()
            : this(
                DependencyResolver.Current.Get<IFingerprintDescriptor>(),
                DependencyResolver.Current.Get<ISpectrumService>(),
                DependencyResolver.Current.Get<IWaveletService>())
        {
        }

        public FingerprintService(IFingerprintDescriptor fingerprintDescriptor, ISpectrumService spectrumService, IWaveletService waveletService)
        {
            this.spectrumService = spectrumService;
            this.waveletService = waveletService;
            this.fingerprintDescriptor = fingerprintDescriptor;
        }

        public List<bool[]> CreateFingerprints(float[] samples, IFingerprintingConfiguration fingerprintingConfiguration)
        {
            float[][] spectrum = spectrumService.CreateLogSpectrogram(samples, fingerprintingConfiguration);
            return CreateFingerprintsFromLogSpectrum(
                spectrum,
                fingerprintingConfiguration.Stride,
                fingerprintingConfiguration.FingerprintLength,
                fingerprintingConfiguration.Overlap,
                fingerprintingConfiguration.TopWavelets);
        }

        private List<bool[]> CreateFingerprintsFromLogSpectrum(float[][] logarithmizedSpectrum, IStride stride, int fingerprintLength, int overlap, int topWavelets)
        {
            List<float[][]> spectralImages = spectrumService.CutLogarithmizedSpectrum(logarithmizedSpectrum, stride, fingerprintLength, overlap);
            
            waveletService.ApplyWaveletTransformInPlace(spectralImages);

            return spectralImages.Select(spectralImage => fingerprintDescriptor.ExtractTopWavelets(spectralImage, topWavelets)).Where(image => !IsSilence(image)).ToList();
        }

        private bool IsSilence(IEnumerable<bool> image)
        {
            return image.All(b => b == false);
        }
    }
}