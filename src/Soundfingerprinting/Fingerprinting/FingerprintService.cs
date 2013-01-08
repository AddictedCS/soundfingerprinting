namespace Soundfingerprinting.Fingerprinting
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Soundfingerprinting.Audio.Models;
    using Soundfingerprinting.Audio.Services;
    using Soundfingerprinting.Audio.Strides;
    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Fingerprinting.Wavelets;
    using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;

    public class FingerprintService : IFingerprintService
    {
        private readonly IWaveletDecomposition waveletDecomposition;

        private readonly IFingerprintDescriptor fingerprintDescriptor;

        private readonly IAudioService audioService;

        public FingerprintService(
            IAudioService audioService,
            IFingerprintDescriptor fingerprintDescriptor,
            IWaveletDecomposition waveletDecomposition)
        {
            this.waveletDecomposition = waveletDecomposition;
            this.fingerprintDescriptor = fingerprintDescriptor;
            this.audioService = audioService;
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
                    WdftSize = configuration.WdftSize
                };

            float[][] spectrum = audioService.CreateLogSpectrogram(
                samples, configuration.WindowFunction, audioServiceConfiguration);

            return CreateFingerprintsFromSpectrum(
                spectrum,
                configuration.Stride,
                configuration.FingerprintLength,
                configuration.Overlap,
                configuration.LogBins,
                configuration.TopWavelets);
        }

        private List<bool[]> CreateFingerprintsFromSpectrum(
            float[][] spectrum, IStride stride, int fingerprintLength, int overlap, int logBins, int topWavelets)
        {
            int start = stride.FirstStrideSize / overlap;
            List<bool[]> fingerprints = new List<bool[]>();

            int width = spectrum.GetLength(0);
            while (start + fingerprintLength < width)
            {
                float[][] frames = new float[fingerprintLength][];
                for (int i = 0; i < fingerprintLength; i++)
                {
                    frames[i] = new float[logBins];
                    Array.Copy(spectrum[start + i], frames[i], logBins);
                }

                start += fingerprintLength + (stride.StrideSize / overlap);
                waveletDecomposition.DecomposeImageInPlace(frames); /*Compute wavelets*/
                bool[] image = fingerprintDescriptor.ExtractTopWavelets(frames, topWavelets);
                fingerprints.Add(image);
            }

            return fingerprints;
        }
    }
}