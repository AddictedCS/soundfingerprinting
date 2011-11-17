// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using Soundfingerprinting.Fingerprinting.FFT;

namespace Soundfingerprinting.Fingerprinting.Windows
{
    /// <summary>
    ///   Window function for spectrogram computing
    /// </summary>
    public interface IWindowFunction
    {
        /// <summary>
        ///   Window the outer space in place
        /// </summary>
        /// <param name = "outerspace">Array to be windowed</param>
        /// <param name = "windowLength">Window length</param>
        void WindowInPlace(float[] outerspace, int windowLength);

        /// <summary>
        ///   Window the outer space in place
        /// </summary>
        /// <param name = "outerspace">Array to be windowed</param>
        /// <param name = "windowLength">Window length</param>
        void WindowInPlace(Complex[] outerspace, int windowLength);

        /// <summary>
        ///   Gets the corresponding window function
        /// </summary>
        /// <param name = "length">Length of the window</param>
        /// <returns>Window function</returns>
        double[] GetWindow(int length);
    }
}