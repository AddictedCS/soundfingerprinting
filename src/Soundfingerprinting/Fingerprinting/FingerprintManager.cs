namespace Soundfingerprinting.Fingerprinting
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Soundfingerprinting.AudioProxies;
    using Soundfingerprinting.AudioProxies.Strides;
    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Fingerprinting.FFT;
    using Soundfingerprinting.Fingerprinting.Wavelets;
    using Soundfingerprinting.Fingerprinting.Windows;
    using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;

    public interface IFingerprintService
    {
        Task<List<bool[]>> Process(IWorkUnitParameterObject workUnitParameterObject);
    }

    public class FingerprintService : IFingerprintService
    {
        public const double HumanAuditoryThreshold = 2 * 0.000001; // 2*10^-5 Pa

        // normalize power (volume) of a wave file.
        // minimum and maximum rms to normalize from.
        private const float MinRms = 0.1f;

        private const float MaxRms = 3;

        private readonly IWaveletDecomposition waveletDecomposition;

        private readonly IWindowFunction windowFunction;

        private readonly IFingerprintDescriptor fingerprintDescriptor;

        private readonly IAudioService audioService;

        public FingerprintService(
            IAudioService audioService,
            IFingerprintDescriptor fingerprintDescriptor,
            IWindowFunction windowFunction,
            IWaveletDecomposition waveletDecomposition)
        {
            this.waveletDecomposition = waveletDecomposition;
            this.fingerprintDescriptor = fingerprintDescriptor;
            this.windowFunction = windowFunction;
            this.audioService = audioService;
        }


        public float[][] CreateSpectrogram(string filename, int sampleRate, int overlap, int wdftSize)
        {
            // read 5512 Hz, Mono, PCM, with a specific proxy
            float[] samples = audioService.ReadMonoFromFile(filename, sampleRate, 0, 0);

            NormalizeInPlace(samples);

            int width = (samples.Length - wdftSize) / overlap; /*width of the image*/
            float[][] frames = new float[width][];
            float[] complexSignal = new float[2 * wdftSize]; /*even - Re, odd - Img*/
            double[] windowArray = windowFunction.GetWindow(wdftSize);
            for (int i = 0; i < width; i++)
            {
                // take 371 ms each 11.6 ms (2048 samples each 64 samples)
                for (int j = 0; j < wdftSize; j++)
                {
                    complexSignal[2 * j] = (float)(windowArray[j] * samples[(i * overlap) + j]);
                    /*Weight by Hann Window*/
                    complexSignal[(2 * j) + 1] = 0;
                }

                Fourier.FFT(complexSignal, wdftSize, FourierDirection.Forward);

                float[] band = new float[(wdftSize / 2) + 1];
                for (int j = 0; j < (wdftSize / 2) + 1; j++)
                {
                    double re = complexSignal[2 * j];
                    double img = complexSignal[(2 * j) + 1];
                    re /= (float)wdftSize / 2;
                    img /= (float)wdftSize / 2;
                    band[j] = (float)Math.Sqrt((re * re) + (img * img));
                }

                frames[i] = band;
            }

            return frames;
        }

        public Task<List<bool[]>> Process(IWorkUnitParameterObject workUnitParameterObject)
        {
            Debug.Assert(
                workUnitParameterObject.AudioSamples != null
                || string.IsNullOrEmpty(workUnitParameterObject.PathToAudioFile) == false,
                "Audio samples or Path to file has to be specified");

            if (!string.IsNullOrEmpty(workUnitParameterObject.PathToAudioFile))
            {
                return
                    Task.Factory.StartNew(
                        () =>
                        CreateFingerprints(
                            workUnitParameterObject.PathToAudioFile,
                            workUnitParameterObject.MillisecondsToProcess,
                            workUnitParameterObject.StartAtMilliseconds,
                            workUnitParameterObject.FingerprintingConfig));
            }

            return
                Task.Factory.StartNew(
                    () =>
                    CreateFingerprints(
                        workUnitParameterObject.AudioSamples, workUnitParameterObject.FingerprintingConfig));
        }

        private List<bool[]> CreateFingerprints(float[] samples, IFingerprintingConfig config)
        {
            float[][] spectrum = CreateLogSpectrogram(
                samples, config.Overlap, config.WdftSize, GenerateLogFrequencies(config), config.LogBins);
            return CreateFingerprintsFromSpectrum(
                spectrum, config.Stride, config.FingerprintLength, config.Overlap, config.LogBins, config.TopWavelets);
        }

        private List<bool[]> CreateFingerprints(
            string pathToAudioFile, int howManyMilliseconds, int startAtMilliseconds, IFingerprintingConfig config)
        {
            float[] samples = audioService.ReadMonoFromFile(
                pathToAudioFile, config.SampleRate, howManyMilliseconds, startAtMilliseconds);
            return CreateFingerprints(samples, config);
        }

        private float[][] CreateLogSpectrogram(
            float[] samples, int overlap, int wdftSize, int[] logFrequenciesIndex, int logBins)
        {
            NormalizeInPlace(samples);
            int width = (samples.Length - wdftSize) / overlap; /*width of the image*/
            float[][] frames = new float[width][];
            float[] complexSignal = new float[2 * wdftSize]; /*even - Re, odd - Img*/
            double[] windowArray = windowFunction.GetWindow(wdftSize);
            for (int i = 0; i < width; i++)
            {
                // take 371 ms each 11.6 ms (2048 samples each 64 samples)
                for (int j = 0; j < wdftSize /*2048*/; j++)
                {
                    complexSignal[(2 * j)] = (float)(windowArray[j] * samples[(i * overlap) + j]);
                        /*Weight by Hann Window*/
                    complexSignal[(2 * j) + 1] = 0;
                }

                // FFT transform for gathering the spectrum
                Fourier.FFT(complexSignal, wdftSize, FourierDirection.Forward);
                frames[i] = ExtractLogBins(complexSignal, logFrequenciesIndex, logBins);
            }

            return frames;
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

        private float[] ExtractLogBins(float[] spectrum, int[] logFrequenciesIndex, int logBins)
        {
            float[] sumFreq = new float[logBins]; /*32*/
            for (int i = 0; i < logBins; i++)
            {
                int lowBound = logFrequenciesIndex[i];
                int higherBound = logFrequenciesIndex[i + 1];

                for (int k = lowBound; k < higherBound; k++)
                {
                    double re = spectrum[2 * k];
                    double img = spectrum[(2 * k) + 1];
                    sumFreq[i] += (float)Math.Sqrt((re * re) + (img * img));
                }

                sumFreq[i] = sumFreq[i] / (higherBound - lowBound);
            }

            return sumFreq;
        }

        /// <summary>
        /// Get logarithmically spaced indices
        /// </summary>
        /// <param name="config">
        /// The configuration for log frequencies
        /// </param>
        /// <returns>
        /// Log indexes
        /// </returns>
        private int[] GenerateLogFrequencies(IFingerprintingConfig config)
        {
            double logMin = Math.Log(config.MinFrequency, config.LogBase);
            double logMax = Math.Log(config.MaxFrequency, config.LogBase);
            double delta = (logMax - logMin) / config.LogBins;

            int[] indexes = new int[config.LogBins + 1];
            double accDelta = 0;
            for (int i = 0; i <= config.LogBins /*32 octaves*/; ++i)
            {
                float freq = (float)Math.Pow(config.LogBase, logMin + accDelta);
                accDelta += delta; // accDelta = delta * i
                /*Find the start index in array from which to start the summation*/
                indexes[i] = FreqToIndex(freq, config.SampleRate, config.WdftSize);
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

        /// <summary>
        ///   Normalizing the input power (volume)
        /// </summary>
        /// <param name = "samples">Samples of a song to be normalized</param>
        private void NormalizeInPlace(IList<float> samples)
        {
            double squares = 0;
            int nsamples = samples.Count;
            for (int i = 0; i < nsamples; i++)
            {
                squares += samples[i] * samples[i];
            }

            // we don't want to normalize by the real RMS, because excessive clipping will occur
            float rms = (float)Math.Sqrt(squares / nsamples) * 10;

            if (rms < MinRms)
            {
                rms = MinRms;
            }

            if (rms > MaxRms)
            {
                rms = MaxRms;
            }

            for (int i = 0; i < nsamples; i++)
            {
                samples[i] /= rms;
                samples[i] = Math.Min(samples[i], 1);
                samples[i] = Math.Max(samples[i], -1);
            }
        }
    }
}