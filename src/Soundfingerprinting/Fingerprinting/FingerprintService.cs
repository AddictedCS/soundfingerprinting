namespace Soundfingerprinting.Fingerprinting
{
    using System.Collections.Generic;

    using Soundfingerprinting.Audio.Strides;
    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Fingerprinting.FFT;
    using Soundfingerprinting.Fingerprinting.Wavelets;
    using Soundfingerprinting.Infrastructure;

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
            
            List<bool[]> fingerprints = new List<bool[]>();
            foreach (var spectralImage in spectralImages)
            {
                bool[] image = fingerprintDescriptor.ExtractTopWavelets(spectralImage, topWavelets);
                fingerprints.Add(image);
            }

            return fingerprints;
        }
    }
}