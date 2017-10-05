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

        void FFTForwardInPlace(float[] data);
    }
}
