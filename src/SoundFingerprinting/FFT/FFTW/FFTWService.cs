namespace SoundFingerprinting.FFT.FFTW
{
    using System;
    using System.Runtime.InteropServices;

    public abstract class FFTWService : IFFTService
    {
        public abstract float[] FFTForward(float[] signal, int startIndex, int length);

        public abstract IntPtr GetOutput(int length);

        public abstract IntPtr GetInput(int length);

        public abstract IntPtr GetFFTPlan(int length, IntPtr input, IntPtr output);

        public abstract void FreeUnmanagedMemory(IntPtr memoryBlock);

        public abstract void FreePlan(IntPtr fftPlan);

        public abstract void Execute(IntPtr fftPlan);
    }
}