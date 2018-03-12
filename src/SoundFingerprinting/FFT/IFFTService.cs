namespace SoundFingerprinting.FFT
{
    internal interface IFFTService
    {
        /// <summary>
        ///   Performs a forward fast Fourier transform on real data input
        /// </summary>
        /// <param name="data">Array to be transformed (not affected by FFT)</param>
        /// <param name="startIndex">Start index</param>
        /// <param name="length">Length of the FFT window</param>
        /// <param name="window">Windowing function to run on input</param>
        /// <returns>Real FFT</returns>
        float[] FFTForward(float[] data, int startIndex, int length, float[] window);
    }

    internal unsafe interface IFFTServiceUnsafe
    {
        /// <summary>
        ///   Perform a forward in-place FFT on an unsafe array with already copied and windowed data
        /// </summary>
        /// <param name="data">Windowed array to run forward FFT on</param>
        /// <param name="length">Length of the input array (has to be a power of 2 entry)</param>
        void FFTForwardInPlace(float* data, int length);
    }
}
