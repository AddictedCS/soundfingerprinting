using System;

namespace Soundfingerprinting.Fingerprinting.FFT.FFTW
{
    using System.Runtime.InteropServices;

    using fftwlib;

    public class FFTWService : IFFTService
    {
        public virtual float[] FFTForward(
            float[] signal, int startIndex, int length, double[] window /*window is disregarded in this implementation*/)
        {
            IntPtr input = GetInput(length);
            IntPtr output = GetOutput(length);
            IntPtr fftPlan = GetFFTPlan(length, input, output);
            Marshal.Copy(signal, startIndex, input, length);
            fftw.execute(fftPlan);
            float[] result = new float[length * 2];
            Marshal.Copy(output, result, 0, length);
            FreeUnmanagedMemory(input);
            FreeUnmanagedMemory(output);
            FreePlan(fftPlan);
            return result;
        }

        protected virtual IntPtr GetOutput(int length)
        {
            return fftw.malloc(8 * length);
        }

        protected virtual IntPtr GetInput(int length)
        {
            return fftw.malloc(4 * length);
        }

        protected virtual IntPtr GetFFTPlan(int length, IntPtr input, IntPtr output)
        {
            return fftwf.dft_r2c_1d(length, input, output, fftw_flags.Estimate);
        }

        protected virtual void FreeUnmanagedMemory(IntPtr memoryBlock)
        {
            fftw.free(memoryBlock);
        }

        protected virtual void FreePlan(IntPtr fftPlan)
        {
            fftw.destroy_plan(fftPlan);
        }
    }
}
