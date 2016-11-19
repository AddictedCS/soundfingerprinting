namespace SoundFingerprinting.FFT.FFTW
{
    using System;

    public abstract class FFTWService : IFFTService, IDisposable
    {
        public abstract float[] FFTForward(float[] data, int startIndex, int length, float[] window);

        public abstract IntPtr GetOutput(int length);

        public abstract IntPtr GetInput(int length);

        public abstract IntPtr GetFFTPlan(int length, IntPtr input, IntPtr output);

        public abstract void FreeUnmanagedMemory(IntPtr memoryBlock);

        public abstract void FreePlan(IntPtr fftPlan);

        public abstract void Execute(IntPtr fftPlan);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool isDisposing);

        protected void Window(float[] data, float[] window)
        {
            for (int i = 0; i < window.Length; ++i)
            {
                data[i] = data[i] * window[i];
            }
        }
    }
}