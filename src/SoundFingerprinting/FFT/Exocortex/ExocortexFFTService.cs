namespace SoundFingerprinting.FFT.Exocortex
{
    public class ExocortexFFTService : IFFTService
    {
        private readonly object lockObject = new object();

        public float[] FFTForward(float[] signal, int startIndex, int length)
        {
            float[] complexSignal = new float[2 * length]; /*even - Re, odd - Img, thats how Exocortex works*/

            // take 371 ms each 11.6 ms (2048 samples each 64 samples)
            for (int i = 0; i < length /*2048*/; i++)
            {
                complexSignal[2 * i] = signal[startIndex + i];
                complexSignal[(2 * i) + 1] = 0;
            }

            lock (lockObject)
            {
                Fourier.FFT(complexSignal, length, FourierDirection.Forward);
            }

            return complexSignal;
        }
    }
}
