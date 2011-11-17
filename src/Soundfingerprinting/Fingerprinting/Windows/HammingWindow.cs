// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using Soundfingerprinting.Fingerprinting.FFT;

namespace Soundfingerprinting.Fingerprinting.Windows
{
    /// <summary>
    ///   Hamming window class
    /// </summary>
    public class HammingWindow : IWindowFunction
    {
        #region IWindowFunction Members

        /// <summary>
        ///   Perform windowing on a signal in place
        /// </summary>
        /// <param name = "outerspace">Signal to be windowed</param>
        /// <param name = "length">Window length</param>
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
            for (int i = 0, n = length; i < n; i++)
                outerspace[i] *= (float) (0.54 - 0.46*Math.Cos(2*Math.PI*i/(n - 1)));
        }

        /// <summary>
        ///   Perform windowing on a signal in place
        /// </summary>
        /// <param name = "outerspace">Signal to be windowed</param>
        /// <param name = "length">Window length</param>
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
            for (int i = 0, n = length; i < n; i++)
                outerspace[i] *= 0.54 - 0.46*Math.Cos(2*Math.PI*i/(n - 1));
        }

        /// <summary>
        ///   Get window array
        /// </summary>
        /// <param name = "length">Length of the array to be returned</param>
        /// <returns></returns>
        public double[] GetWindow(int length)
        {
            double[] array = new double[length];
            for (int i = 0; i < length; i++)
                array[i] = 0.54 - 0.46*Math.Cos(2*Math.PI*i/(length - 1));
            return array;
        }

        #endregion
    }
}