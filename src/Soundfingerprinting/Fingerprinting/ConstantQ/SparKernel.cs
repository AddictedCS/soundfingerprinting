namespace Soundfingerprinting.Fingerprinting.ConstantQ
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Soundfingerprinting.Fingerprinting.FFT;
    using Soundfingerprinting.Fingerprinting.Windows;

    public class SparKernel
    {
        /// <summary>
        ///   Number of bins per octave
        /// </summary>
        /// <remarks>
        ///   12 bins generate 32 logarithmically spaced bins
        /// </remarks>
        private readonly int bins;

        private readonly double frequencyRate;

        private readonly double maxFreq;

        private readonly double minFreq;

        /// <summary>
        ///   Threshold for filtering low components
        /// </summary>
        /// <remarks>
        ///   0.0054 for Hamming Window
        ///   0      for Hanning Window
        /// </remarks>
        private readonly double threshold;

        /// <summary>
        ///   Window function used in the algorithm
        /// </summary>
        /// <remarks>
        ///   Hanning window
        /// </remarks>
        private readonly IWindowFunction window;


        /// <summary>
        /// Initializes a new instance of the <see cref="SparKernel"/> class. 
        /// </summary>
        /// <param name="minFreq">
        /// Minimum frequency
        /// </param>
        /// <param name="maxFreq">
        /// Maximum frequency
        /// </param>
        /// <param name="bins">
        /// Number of bins per octave
        /// </param>
        /// <param name="frequencyRate">
        /// Frequency rate
        /// </param>
        /// <param name="threshold">
        /// Threshold value for filtering components
        /// </param>
        /// <param name="function">
        /// Window function
        /// </param>
        public SparKernel(double minFreq, double maxFreq, int bins, double frequencyRate, double threshold, IWindowFunction function)
        {
            this.minFreq = minFreq;
            this.maxFreq = maxFreq;
            this.bins = bins;
            this.frequencyRate = frequencyRate;
            this.threshold = threshold;
            window = function;
        }

        public Complex[][] SparKernelVector
        {
            get
            {
               return GenerateSparKernel();
            }
        }

        /// <summary>
        /// Spar kernel generation
        /// </summary>
        /// <remarks>
        /// The algorithm was rewritten from Matlab implementation
        ///   http://wwwmath.uni-muenster.de/logik/Personen/blankertz/constQ/constQ.html
        /// </remarks>
        /// <returns>
        /// The Soundfingerprinting.Fingerprinting.FFT.Complex[][].
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        private Complex[][] GenerateSparKernel()
        {
            double Q = 1.0 / (Math.Pow(2, 1.0 / bins) - 1);
            int K = (int)Math.Ceiling(bins * Math.Log(maxFreq / minFreq, 2));
            const int FftLen = 2048; // NextPowerOfTwo((int)Math.Ceiling(Q * _frequencyRate / _minFreq));

            Complex[][] specKernel = new Complex[K][];
            for (int k = K; k > 0; k--)
            {
                int len = (int)Math.Ceiling(Q * frequencyRate / (minFreq * Math.Pow(2, (double)(k - 1) / bins)));
                for (int i = 0; i < len; i++)
                {
                    Complex[] tempKernel = new Complex[FftLen];
                    double[] windowArray = this.window.GetWindow(len);
                    System.Numerics.Complex complexI = new System.Numerics.Complex(0, 1);
                    for (int j = 0; j < len; j++)
                    {
                        windowArray[j] /= len;
                        System.Numerics.Complex expValue =
                            System.Numerics.Complex.Exp(2 * Math.PI * complexI * Q * j / len);
                        System.Numerics.Complex value = windowArray[j] * expValue;
                        tempKernel[j] = new Complex(value.Real, value.Imaginary);
                    }
                    
                    Fourier.FFT(tempKernel, tempKernel.Length, FourierDirection.Forward);

                    for (int t = 0; t < FftLen; t++)
                    {
                        if (tempKernel[t].GetModulus() <= threshold)
                        {
                            tempKernel[t] = (Complex)0;
                        }
                    }

                    specKernel[k - 1] = new Complex[FftLen];
                    for (int t = 0; t < FftLen; t++)
                    {
                        specKernel[k - 1][t] = tempKernel[t];
                    }
                }
            }

            for (int i = 0; i < K; i++)
            {
                for (int j = 0; j < FftLen; j++)
                {
                    specKernel[i][j] = new Complex(specKernel[i][j].Re / FftLen, -specKernel[i][j].Im / FftLen);
                }
            }

           return specKernel;
        }
    }
}