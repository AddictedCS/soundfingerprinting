namespace SoundFingerprinting.FFT
{
    public interface IFFTService
    {
        /// <summary>
        /// Performs a forward fast Fourier transform
        /// </summary>
        /// <param name="signal">Array to be transformed</param>
        /// <param name="startIndex">Index to start at</param>
        /// <param name="length">Length of FFT window</param>
        /// <returns>Twice as bigger result with real amd img transforms</returns>
        float[] FFTForward(float[] signal, int startIndex, int length);
    }
}
