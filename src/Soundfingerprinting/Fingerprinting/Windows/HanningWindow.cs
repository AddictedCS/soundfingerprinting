// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using Soundfingerprinting.Fingerprinting.FFT;

namespace Soundfingerprinting.Fingerprinting.Windows
{
    /// <summary>
    ///   Hanning window function
    /// </summary>
    public class HanningWindow : IWindowFunction
    {
        #region IWindowFunction Members

        /// <summary>
        ///   Window the outer space by Hanning window function
        /// </summary>
        /// <param name = "outerspace">Array to be windowed</param>
        /// <param name = "length">Window length</param>
        /// <remarks>
        ///   For additional explanation please consult http://en.wikipedia.org/wiki/Hann_function
        /// </remarks>
        public void WindowInPlace(float[] outerspace, int length)
        {
#if SAFE
            if (outerspace == null)
                throw new ArgumentNullException("outerspace");
            if (outerspace.Length <= 1)
                throw new ArgumentException("Length of outer space parameter should be bigger than 1, otherwise division by zero will occur");
            if (outerspace.Length < length)
                throw new ArgumentException("Length of the outer space parameter should be bigger of equal to window length");
#endif
            //Hanning window of the whole signal
            for (int i = 0, n = length; i < n; i++)
                outerspace[i] *= (float) (0.5*(1 - Math.Cos(2*Math.PI*i/(n - 1))));
        }

        /// <summary>
        ///   Window the outer space by Hanning window function
        /// </summary>
        /// <param name = "outerspace">Array to be windowed</param>
        /// <param name = "length">Window length</param>
        /// <remarks>
        ///   For additional explanation please consult http://en.wikipedia.org/wiki/Hann_function
        /// </remarks>
        public void WindowInPlace(Complex[] outerspace, int length)
        {
#if SAFE
            if (outerspace == null)
                throw new ArgumentNullException("outerspace");
            if (outerspace.Length <= 1)
                throw new ArgumentException("Length of outer space parameter should be bigger than 1, otherwise division by zero will occur");
            if (outerspace.Length < length)
                throw new ArgumentException("Length of the outer space parameter should be bigger of equal to window length");
#endif
            //Hanning window of the whole signal
            for (int i = 0, n = length; i < n; i++)
                outerspace[i] *= 0.5*(1 - Math.Cos(2*Math.PI*i/(n - 1)));
        }

        /// <summary>
        ///   Gets the corresponding window function
        /// </summary>
        /// <param name = "length">Length of the window</param>
        /// <returns>Window function</returns>
        public double[] GetWindow(int length)
        {
            double[] array = new double[length];
            //Hanning window of the whole signal
            for (int i = 0; i < length; i++)
                array[i] = 0.5*(1 - Math.Cos(2*Math.PI*i/(length - 1)));
            return array;
        }

        #endregion
    }
}