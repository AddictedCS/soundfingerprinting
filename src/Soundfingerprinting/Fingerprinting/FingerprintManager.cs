// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com

namespace Soundfingerprinting.Fingerprinting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Soundfingerprinting.AudioProxies;
    using Soundfingerprinting.AudioProxies.Strides;
    using Soundfingerprinting.Fingerprinting.FFT;
    using Soundfingerprinting.Fingerprinting.Wavelets;
    using Soundfingerprinting.Fingerprinting.Windows;

    public interface IFingerprintManager
    {
        /// <summary>
        ///   Create spectrogram of the input file
        /// </summary>
        /// <param name = "proxy">Proxy used to read from file</param>
        /// <param name = "filename">Filename</param>
        /// <param name = "milliseconds"> Milliseconds to process</param>
        /// <param name = "startmilliseconds">Starting point of the processing</param>
        /// <returns>Spectrogram</returns>
        float[][] CreateSpectrogram(IAudio proxy, string filename, int milliseconds, int startmilliseconds);
    }

    public class FingerprintManager : IFingerprintManager
    {
        public const double HumanAuditoryThreshold = 2 * 0.000001; // 2*10^-5 Pa

        /// <summary>
        /// Number of logarithmically spaced bins between the frequency components computed by Fast Fourier Transform.
        /// </summary>
        public const int LogBins = 32;

        // normalize power (volume) of a wave file.
        // minimum and maximum rms to normalize from.
        private const float MinRms = 0.1f;

        private const float MaxRms = 3;

        private readonly double[] windowArray;

        private readonly int[] logFrequenciesIndex;

        private IWindowFunction windowFunction;
       
        public FingerprintManager()
        {
            windowFunction = new HanningWindow();
            WaveletDecomposition = new HaarWavelet();
            FingerprintDescriptor = new FingerprintDescriptor();
            FingerprintLength = 128;
            Overlap = 64;
            SamplesPerFingerprint = FingerprintLength * Overlap;
            WdftSize = 2048;
            MinFrequency = 318;
            MaxFrequency = 2000;
            TopWavelets = 200;
            SampleRate = 5512;
            LogBase = 2;
            logFrequenciesIndex = GenerateLogFrequencies(
                SampleRate, MinFrequency, MaxFrequency, LogBins, WdftSize, LogBase);

            windowArray = windowFunction.GetWindow(WdftSize);
        }

        public event EventHandler<FingerprintManagerEventArgs> UnhandledException;

        public IFingerprintDescriptor FingerprintDescriptor { get; set; }

        /// <summary>
        ///   Wavelet decomposition algorithm
        /// </summary>
        /// <remarks>
        ///   Default <c>HaarWavelet</c>
        /// </remarks>
        public IWaveletDecomposition WaveletDecomposition { get; set; }


        /// <summary>
        ///   Number of samples to read in order to create single fingerprint.
        ///   The granularity is 1.48 seconds
        /// </summary>
        /// <remarks>
        ///   Default = 8192
        /// </remarks>
        public int SamplesPerFingerprint { get; private set; }

        /// <summary>
        ///   Overlap between the sub fingerprints, 11.6 ms
        /// </summary>
        /// <remarks>
        ///   Default = 64
        /// </remarks>
        public int Overlap { get; private set; }

        /// <summary>
        ///   Size of the WDFT block, 371 ms
        /// </summary>
        /// <remarks>
        ///   Default = 2048
        /// </remarks>
        public int WdftSize { get; private set; }

        /// <summary>
        ///   Frequency range which is taken into account
        /// </summary>
        /// <remarks>
        ///   Default = 318
        /// </remarks>
        public int MinFrequency { get; set; }

        /// <summary>
        ///   Frequency range which is taken into account
        /// </summary>
        /// <remarks>
        ///   Default = 2000
        /// </remarks>
        public int MaxFrequency { get; set; }

        /// <summary>
        ///   Number of Top wavelets to consider
        /// </summary>
        /// <remarks>
        ///   Default = 200
        /// </remarks>
        public int TopWavelets { get; set; }

        /// <summary>
        ///   Sample rate
        /// </summary>
        /// <remarks>
        ///   Default = 5512
        /// </remarks>
        public int SampleRate { get; set; }

        /// <summary>
        ///   Log base used for computing the logarithmically spaced frequency bins
        /// </summary>
        /// <remarks>
        ///   Default = 10
        /// </remarks>
        public double LogBase { get; set; }

        /// <summary>
        ///   Fingerprint's length
        /// </summary>
        public int FingerprintLength { get; set; }

        /// <summary>
        ///   Create spectrogram of the input file
        /// </summary>
        /// <param name = "proxy">Proxy used to read from file</param>
        /// <param name = "filename">Filename</param>
        /// <param name = "milliseconds">Milliseconds to process</param>
        /// <param name = "startmilliseconds">Starting point of the processing</param>
        /// <returns>Spectrogram</returns>
        public float[][] CreateSpectrogram(IAudio proxy, string filename, int milliseconds, int startmilliseconds)
        {
            // read 5512 Hz, Mono, PCM, with a specific proxy
            float[] samples = proxy.ReadMonoFromFile(filename, SampleRate, milliseconds, startmilliseconds);

            NormalizeInPlace(samples);

            int overlap = Overlap;
            int wdftSize = WdftSize;
            int width = (samples.Length - wdftSize) / overlap; /*width of the image*/
            float[][] frames = new float[width][];
            float[] complexSignal = new float[2 * wdftSize]; /*even - Re, odd - Img*/
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

        /// <summary>
        ///   Create logarithmically spaced spectrogram out of the input samples (spaced according to manager's parameters)
        /// </summary>
        /// <param name = "samples">Samples of a song</param>
        /// <returns>Logarithmically spaced bins within the power spectrum</returns>
        public float[][] CreateLogSpectrogram(float[] samples)
        {
            NormalizeInPlace(samples);
            int overlap = Overlap;
            int wdftSize = WdftSize;
            int width = (samples.Length - wdftSize) / overlap; /*width of the image*/
            float[][] frames = new float[width][];
            float[] complexSignal = new float[2 * wdftSize]; /*even - Re, odd - Img*/
            for (int i = 0; i < width; i++)
            {
                // take 371 ms each 11.6 ms (2048 samples each 64 samples)
                for (int j = 0; j < wdftSize /*2048*/; j++)
                {
                    complexSignal[(2 * j)] = (float)(windowArray[j] * samples[(i * overlap) + j]); /*Weight by Hann Window*/
                    complexSignal[(2 * j) + 1] = 0;
                }

                // FFT transform for gathering the spectrum
                Fourier.FFT(complexSignal, wdftSize, FourierDirection.Forward);
                frames[i] = ExtractLogBins(complexSignal);
            }

            return frames;
        }

        /// <summary>
        ///   Create log-spectrogram (spaced according to manager's parameters)
        /// </summary>
        /// <param name = "proxy">Proxy used in generating the spectrogram</param>
        /// <param name = "filename">Filename to be processed</param>
        /// <param name = "milliseconds">Milliseconds to be analyzed</param>
        /// <param name = "startmilliseconds">Starting point</param>
        /// <returns>Logarithmically spaced bins within the power spectrum</returns>
        public float[][] CreateLogSpectrogram(IAudio proxy, string filename, int milliseconds, int startmilliseconds)
        {
            // read 5512 Hz, Mono, PCM, with a specific proxy
            float[] samples = proxy.ReadMonoFromFile(filename, SampleRate, milliseconds, startmilliseconds);
            return CreateLogSpectrogram(samples);
        }


        /// <summary>
        ///   Create fingerprints according to the Google's researchers algorithm
        /// </summary>
        /// <param name = "proxy">Proxy used in reading from file</param>
        /// <param name = "filename">Filename to be analyzed</param>
        /// <param name = "stride">Stride between 2 consecutive fingerprints</param>
        /// <param name = "milliseconds">Milliseconds to analyze</param>
        /// <param name = "startmilliseconds">Starting point of analysis</param>
        /// <returns>Fingerprint signatures</returns>
        public List<bool[]> CreateFingerprints(
            IAudio proxy, string filename, IStride stride, int milliseconds, int startmilliseconds)
        {
            float[][] spectrum = CreateLogSpectrogram(proxy, filename, milliseconds, startmilliseconds);
            return CreateFingerprints(spectrum, stride);
        }

        /// <summary>
        ///   Create fingerprints from already written samples
        /// </summary>
        /// <param name = "samples">Samples from a song</param>
        /// <param name = "stride">Stride between 2 consecutive fingerprints</param>
        /// <returns>Fingerprint signatures</returns>
        public List<bool[]> CreateFingerprints(float[] samples, IStride stride)
        {
            float[][] spectrum = CreateLogSpectrogram(samples);
            return CreateFingerprints(spectrum, stride);
        }

        /// <summary>
        ///   Create fingerprints gathered from one specific song
        /// </summary>
        /// <param name = "proxy">Proxy used in reading the audio file</param>
        /// <param name = "filename">Filename</param>
        /// <param name = "stride">Stride used in fingerprint creation</param>
        /// <returns>List of fingerprint signatures</returns>
        public List<bool[]> CreateFingerprints(IAudio proxy, string filename, IStride stride)
        {
            return CreateFingerprints(proxy, filename, stride, 0, 0);
        }

        protected virtual void OnUnhandledException(FingerprintManagerEventArgs eventArg)
        {
            EventHandler<FingerprintManagerEventArgs> temp = UnhandledException;
            if (temp != null)
            {
                temp.Invoke(this, eventArg);
            }
        }

        /// <summary>
        ///   Create fingerprints according to the Google's researchers algorithm
        /// </summary>
        /// <param name = "spectrum">Spectrogram of the song</param>
        /// <param name = "stride">Stride between 2 consecutive fingerprints</param>
        /// <returns>Fingerprint signatures</returns>
        private List<bool[]> CreateFingerprints(float[][] spectrum, IStride stride)
        {
            int fingerprintLength = FingerprintLength;
            int overlap = Overlap;
            int logbins = LogBins;
            int start = stride.GetFirstStride() / overlap;
            List<bool[]> fingerprints = new List<bool[]>();

            int width = spectrum.GetLength(0);
            while (start + fingerprintLength < width)
            {
                float[][] frames = new float[fingerprintLength][];
                for (int i = 0; i < fingerprintLength; i++)
                {
                    frames[i] = new float[logbins];
                    Array.Copy(spectrum[start + i], frames[i], logbins);
                }

                start += fingerprintLength + (stride.GetStride() / overlap);
                WaveletDecomposition.DecomposeImageInPlace(frames); /*Compute wavelets*/
                bool[] image = FingerprintDescriptor.ExtractTopWavelets(frames, TopWavelets);
                fingerprints.Add(image);
            }

            return fingerprints;
        }

        /// <summary>
        ///   Logarithmic spacing of a frequency in a linear domain
        /// </summary>
        /// <param name = "spectrum">Spectrum to space</param>
        /// <returns>Logarithmically spaced signal</returns>
        private float[] ExtractLogBins(float[] spectrum)
        {
            float[] sumFreq = new float[LogBins]; /*32*/
            for (int i = 0; i < LogBins; i++)
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
        ///   Get logarithmically spaced indices
        /// </summary>
        /// <param name = "sampleRate">Signal's sample rate</param>
        /// <param name = "minFreq">Min frequency</param>
        /// <param name = "maxFreq">Max frequency</param>
        /// <param name = "logBins">Number of logarithmically spaced bins</param>
        /// <param name = "fftSize">FFT Size</param>
        /// <param name = "logarithmicBase">Logarithm base</param>
        /// <returns>
        ///  Log indexes
        /// </returns>
        private int [] GenerateLogFrequencies(
            int sampleRate, int minFreq, int maxFreq, int logBins, int fftSize, double logarithmicBase)
        {
            double logMin = Math.Log(minFreq, logarithmicBase);
            double logMax = Math.Log(maxFreq, logarithmicBase);
            double delta = (logMax - logMin) / logBins;

            int[] indexes = new int[logBins + 1];
            double accDelta = 0;
            for (int i = 0; i <= logBins /*32 octaves*/; ++i)
            {
                float freq = (float)Math.Pow(logarithmicBase, logMin + accDelta);
                accDelta += delta; // accDelta = delta * i
                /*Find the start index in array from which to start the summation*/
                indexes[i] = FreqToIndex(freq, sampleRate, fftSize);
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