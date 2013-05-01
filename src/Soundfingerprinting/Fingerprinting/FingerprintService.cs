namespace Soundfingerprinting.Fingerprinting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Soundfingerprinting.Audio.Models;
    using Soundfingerprinting.Audio.Services;
    using Soundfingerprinting.Audio.Strides;
    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Fingerprinting.Spectrum;
    using Soundfingerprinting.Fingerprinting.Wavelets;
    using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;
    using Soundfingerprinting.Models;

    public class FingerprintService : IFingerprintService
    {
        private readonly IAudioService audioService;
        private readonly IWaveletService waveletService;
        private readonly ISpectrumService spectrumService;
        private readonly IFingerprintDescriptor fingerprintDescriptor;
        
        public FingerprintService(
            IAudioService audioService,
            IFingerprintDescriptor fingerprintDescriptor,
            IWaveletService waveletService,
            ISpectrumService spectrumService)
        {
            this.waveletService = waveletService;
            this.spectrumService = spectrumService;
            this.fingerprintDescriptor = fingerprintDescriptor;
            this.audioService = audioService;
        }

        public async Task<List<Fingerprint>> ProcessEx(WorkUnitParameterObject details)
        {
            List<bool[]> fingerprints = await Process(details);
            return fingerprints.Select((bools, i) => new Fingerprint { OrderNumber = i, Signature = bools }).ToList();
        }

        public Task<List<bool[]>> Process(WorkUnitParameterObject details)
        {
            if (!string.IsNullOrEmpty(details.PathToAudioFile))
            {
                return Task.Factory.StartNew(() => CreateFingerprintsFromAudioFile(details));
            }

            return Task.Factory.StartNew(() => CreateFingerprintsFromAudioSamples(details.AudioSamples, details));
        }

        private List<bool[]> CreateFingerprintsFromAudioFile(WorkUnitParameterObject param)
        {
            float[] samples = audioService.ReadMonoFromFile(
                param.PathToAudioFile,
                param.FingerprintingConfiguration.SampleRate,
                param.MillisecondsToProcess,
                param.StartAtMilliseconds);

            return CreateFingerprintsFromAudioSamples(samples, param);
        }

        private List<bool[]> CreateFingerprintsFromAudioSamples(float[] samples, WorkUnitParameterObject param)
        {
            IFingerprintingConfiguration configuration = param.FingerprintingConfiguration;
            AudioServiceConfiguration audioServiceConfiguration = new AudioServiceConfiguration
                {
                    LogBins = configuration.LogBins,
                    LogBase = configuration.LogBase,
                    MaxFrequency = configuration.MaxFrequency,
                    MinFrequency = configuration.MinFrequency,
                    Overlap = configuration.Overlap,
                    SampleRate = configuration.SampleRate,
                    WdftSize = configuration.WdftSize,
                    NormalizeSignal = configuration.NormalizeSignal,
                    UseDynamicLogBase = configuration.UseDynamicLogBase
                };

            float[][] spectrum = audioService.CreateLogSpectrogram(
                samples, configuration.WindowFunction, audioServiceConfiguration);

            return this.CreateFingerprintsFromLogSpectrum(
                spectrum,
                configuration.Stride,
                configuration.FingerprintLength,
                configuration.Overlap,
                configuration.TopWavelets);
        }

        private List<bool[]> CreateFingerprintsFromLogSpectrum(
            float[][] logarithmizedSpectrum, IStride stride, int fingerprintLength, int overlap, int topWavelets)
        {
            List<float[][]> spectralImages = spectrumService.CutLogarithmizedSpectrum(
                logarithmizedSpectrum, stride, fingerprintLength, overlap);

            waveletService.ApplyWaveletTransformInPlace(spectralImages);

            List<bool[]> fingerprints = new List<bool[]>();
            foreach (var waveletDecomposedImage in spectralImages)
            {
                bool[] image = fingerprintDescriptor.ExtractTopWavelets(waveletDecomposedImage, topWavelets);
                fingerprints.Add(image);
            }

            return fingerprints;
        }
    }
}