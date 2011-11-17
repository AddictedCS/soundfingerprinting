// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using Soundfingerprinting.Fingerprinting.FFT;
using Soundfingerprinting.Fingerprinting.Windows;

namespace Soundfingerprinting.Fingerprinting.ConstantQ
{
    /// <summary>
    ///   Class for generating spar kernels
    /// </summary>
    public class SparKernel
    {
        /// <summary>
        ///   Number of bins per octave
        /// </summary>
        /// <remarks>
        ///   12 bins generate 32 logarithmically spaced bins
        /// </remarks>
        private readonly int _bins;

        /// <summary>
        ///   Frequency rate
        /// </summary>
        /// <remarks>
        ///   5512 Hz
        /// </remarks>
        private readonly double _frequencyRate;

        /// <summary>
        ///   Lock object for solving multithreading issues
        /// </summary>
        private readonly object _lockObject = new object();

        /// <summary>
        ///   Max frequency
        /// </summary>
        /// <remarks>
        ///   2000 Hz
        /// </remarks>
        private readonly double _maxFreq;

        /// <summary>
        ///   Min frequency
        /// </summary>
        /// <remarks>
        ///   318 Hz
        /// </remarks>
        private readonly double _minFreq;

        /// <summary>
        ///   Threshold for filtering low components
        /// </summary>
        /// <remarks>
        ///   0.0054 for Hamming Window
        ///   0      for Hanning Window
        /// </remarks>
        private readonly double _threshold;

        /// <summary>
        ///   Window function used in the algorithm
        /// </summary>
        /// <remarks>
        ///   Hanning window
        /// </remarks>
        private readonly IWindowFunction _window;

        /// <summary>
        ///   Spar kernel
        /// </summary>
        private Complex[][] _sparKernel;


        /// <summary>
        ///   Constructor for spark kernel algorithm object
        /// </summary>
        /// <param name = "minFreq">Minimum frequency</param>
        /// <param name = "maxFreq">Maximum frequency</param>
        /// <param name = "bins">Number of bins per octave</param>
        /// <param name = "frequencyRate">Frequency rate</param>
        /// <param name = "threshold">Threshold value for filtering components</param>
        /// <param name = "function">Window function</param>
        public SparKernel(double minFreq, double maxFreq, int bins, double frequencyRate, double threshold, IWindowFunction function)
        {
            _minFreq = minFreq;
            _maxFreq = maxFreq;
            _bins = bins;
            _frequencyRate = frequencyRate;
            _threshold = threshold;
            _window = function;
            Action action = GenerateSparKernel;
            action.BeginInvoke(action.EndInvoke, action);
        }

        /// <summary>
        ///   Get sparse kernel vector
        /// </summary>
        public Complex[][] SparKernelVector
        {
            get
            {
                if (_sparKernel == null)
                    GenerateSparKernel();
                return _sparKernel;
            }
        }

        /// <summary>
        ///   Spar kernel generation
        /// </summary>
        /// <remarks>
        ///   The algorithm was rewritten from Matlab implementation
        ///   http://wwwmath.uni-muenster.de/logik/Personen/blankertz/constQ/constQ.html
        /// </remarks>
        /*
                function sparKernel= sparseKernel(minFreq, maxFreq, bins, fs, thresh)
                if nargin<5 thresh= 0.0054; end % for Hamming window
                Q= 1/(2^(1/bins)-1);
                K= ceil( bins * log2(maxFreq/minFreq) ); 
                fftLen= 2048; %2^nextpow2( ceil(Q*fs/minFreq) );
                tempKernel= zeros(fftLen, 1);
                sparKernel= [];
                for k= K:-1:1;
                    len= ceil( Q * fs / (minFreq*2^((k-1)/bins)) ); 
                    tempKernel(1:len)= ...
                        hamming(len)/len .* exp(2*pi*1i*Q*(0:len-1)'/len); 
                    specKernel= fft(tempKernel); 
                    specKernel(find(abs(specKernel)<=thresh))= 0;
                    sparKernel= sparse([specKernel sparKernel]);
               end
               sparKernel= conj(sparKernel) / fftLen; 
         */
        private void GenerateSparKernel()
        {
            lock (_lockObject)
            {
                if (_sparKernel != null) return;
                double Q = 1.0/(Math.Pow(2, 1.0/_bins) - 1);
                int K = (int) Math.Ceiling(_bins*Math.Log(_maxFreq/_minFreq, 2));
                int fftLen = 2048; // NextPowerOfTwo((int)Math.Ceiling(Q * _frequencyRate / _minFreq));

                Complex[][] specKernel = new Complex[K][];
                for (int k = K; k > 0; k--)
                {
                    int len = (int) Math.Ceiling(Q*_frequencyRate/(_minFreq*Math.Pow(2, (double) (k - 1)/_bins)));
                    for (int i = 0; i < len; i++)
                    {
                        Complex[] tempKernel = new Complex[fftLen];
                        double[] window = _window.GetWindow(len);
                        System.Numerics.Complex complexI = new System.Numerics.Complex(0, 1);
                        for (int j = 0; j < len; j++)
                        {
                            window[j] /= len;
                            System.Numerics.Complex expValue = System.Numerics.Complex.Exp(2*Math.PI*complexI*Q*j/len);
                            System.Numerics.Complex value = window[j]*expValue;
                            tempKernel[j] = new Complex(value.Real, value.Imaginary);
                        }
                        Fourier.FFT(tempKernel, tempKernel.Length, FourierDirection.Forward);
                        //FourierTransform.FFT(tempKernel, FourierTransform.Direction.Backward);
                        for (int t = 0; t < fftLen; t++)
                            if (tempKernel[t].GetModulus() <= _threshold)
                                tempKernel[t] = (Complex) 0;
                        specKernel[k - 1] = new Complex[fftLen];
                        for (int t = 0; t < fftLen; t++)
                            specKernel[k - 1][t] = tempKernel[t];
                    }
                }

                for (int i = 0; i < K; i++)
                    for (int j = 0; j < fftLen; j++)
                        specKernel[i][j] = new Complex(specKernel[i][j].Re/fftLen, -specKernel[i][j].Im/fftLen);
                _sparKernel = specKernel;
            }
        }

        /// <summary>
        ///   Finds next power of 2 value
        /// </summary>
        /// <param name = "v">Initial value</param>
        /// <returns>Power of 2 value</returns>
        private static int NextPowerOfTwo(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            return ++v;
        }
    }
}