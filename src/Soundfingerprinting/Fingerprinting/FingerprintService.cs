namespace Soundfingerprinting.Fingerprinting
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Soundfingerprinting.AudioProxies;
    using Soundfingerprinting.AudioProxies.Strides;
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

        public Task<List<bool[]>> Process(IWorkUnitParameterObject details)
        {
            if (!string.IsNullOrEmpty(details.PathToAudioFile))
            {
                return Task.Factory.StartNew(() => CreateFingerprintsFromAudioFile(details));
            }

            return Task.Factory.StartNew(() => CreateFingerprintsFromAudioSamples(details.AudioSamples, details));
        }

        private List<bool[]> CreateFingerprintsFromAudioFile(IWorkUnitParameterObject param)
        {
            float[] samples = audioService.ReadMonoFromFile(
                param.PathToAudioFile,
                param.FingerprintingConfiguration.SampleRate,
                param.MillisecondsToProcess,
                param.StartAtMilliseconds);

            return CreateFingerprintsFromAudioSamples(samples, param);
        }


        private List<bool[]> CreateFingerprintsFromAudioSamples(float[] samples, IWorkUnitParameterObject param)
        {
            IFingerprintingConfiguration configuration = param.FingerprintingConfiguration;
            float[][] spectrum = audioService.CreateLogSpectrogram(
                samples,
                configuration.Overlap,
                configuration.WdftSize,
                GenerateLogFrequencies(configuration),
                configuration.WindowFunction.GetWindow(configuration.WdftSize),
                configuration.LogBins);

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
            int start = stride.GetFirstStride() / overlap;
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

                start += fingerprintLength + (stride.GetStride() / overlap);
                waveletDecomposition.DecomposeImageInPlace(frames); /*Compute wavelets*/
                bool[] image = fingerprintDescriptor.ExtractTopWavelets(frames, topWavelets);
                fingerprints.Add(image);
            }

            return fingerprints;
        }

        /// <summary>
        /// Get logarithmically spaced indices
        /// </summary>
        /// <param name="configuration">
        /// The configuration for log frequencies
        /// </param>
        /// <returns>
        /// Log indexes
        /// </returns>
        private int[] GenerateLogFrequencies(IFingerprintingConfiguration configuration)
        {
            double logMin = Math.Log(configuration.MinFrequency, configuration.LogBase);
            double logMax = Math.Log(configuration.MaxFrequency, configuration.LogBase);
            double delta = (logMax - logMin) / configuration.LogBins;

            int[] indexes = new int[configuration.LogBins + 1];
            double accDelta = 0;
            for (int i = 0; i <= configuration.LogBins /*32 octaves*/; ++i)
            {
                float freq = (float)Math.Pow(configuration.LogBase, logMin + accDelta);
                accDelta += delta; // accDelta = delta * i
                /*Find the start index in array from which to start the summation*/
                indexes[i] = FreqToIndex(freq, configuration.SampleRate, configuration.WdftSize);
            }

            return indexes;
        }

        /*
         * An array of WDFT [0, 2048], contains a range of [0, 5512] frequency components.
         * Only 1024 contain actual data. In order to find the Index, the fraction is found by dividing the frequency by max frequency
         */

        /// <summary>
        ///   Gets the index in the spectrum vector from according to the starting frequency specified as the parameter
        /// </summary>
        /// <param name = "freq">Frequency to be found in the spectrum vector [E.g. 300Hz]</param>
        /// <param name = "sampleRate">Frequency rate at which the signal was processed [E.g. 5512Hz]</param>
        /// <param name = "spectrumLength">Length of the spectrum [2048 elements generated by WDFT from which only 1024 are with the actual data]</param>
        /// <returns>Index of the frequency in the spectrum array</returns>
        /// <remarks>
        /// The Bandwidth of the spectrum runs from 0 until SampleRate / 2 [E.g. 5512 / 2]
        ///   Important to remember:
        ///   N points in time domain correspond to N/2 + 1 points in frequency domain
        ///   E.g. 300 Hz applies to 112'th element in the array
        /// </remarks>
        private int FreqToIndex(float freq, int sampleRate, int spectrumLength)
        {
            /*N sampled points in time correspond to [0, N/2] frequency range */
            float fraction = freq / ((float)sampleRate / 2);
            /*DFT N points defines [N/2 + 1] frequency points*/
            int i = (int)Math.Round(((spectrumLength / 2) + 1) * fraction);
            return i;
        }
    }
}