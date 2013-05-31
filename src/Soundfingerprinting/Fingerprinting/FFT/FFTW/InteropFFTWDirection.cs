namespace Soundfingerprinting.Fingerprinting.FFT.FFTW
{
    /// <summary>
    /// Defines direction of operation
    /// </summary>
    public enum InteropFFTWDirection : int
    {
        /// <summary>
        /// Computes a regular DFT
        /// </summary>
        Forward = -1,

        /// <summary>
        /// Computes the inverse DFT
        /// </summary>
        Backward = 1
    }
}