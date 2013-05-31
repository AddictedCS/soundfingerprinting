namespace Soundfingerprinting.Fingerprinting.FFT.FFTW
{
    using System;
    using System.Runtime.InteropServices;

    public class FFTWService : IFFTService
    {
        public virtual float[] FFTForward(float[] signal, int startIndex, int length, double[] window)
        {
            IntPtr input = GetInput(length);
            IntPtr output = GetOutput(length);
            IntPtr fftPlan = GetFFTPlan(length, input, output);
            float[] applyTo = new float[length];
            Array.Copy(signal, startIndex, applyTo, 0, length);
            for (int i = 0; i < length; i++)
            {
                applyTo[i] = (float)(applyTo[i] * window[i]);
            }

            Marshal.Copy(applyTo, 0, input, length);
            InteropFFTW.execute(fftPlan);
            float[] result = new float[length * 2];
            Marshal.Copy(output, result, 0, length);
            FreeUnmanagedMemory(input);
            FreeUnmanagedMemory(output);
            FreePlan(fftPlan);
            return result;
        }

        protected virtual IntPtr GetOutput(int length)
        {
            return InteropFFTW.malloc(8 * length);
        }

        protected virtual IntPtr GetInput(int length)
        {
            return InteropFFTW.malloc(4 * length);
        }

        protected virtual IntPtr GetFFTPlan(int length, IntPtr input, IntPtr output)
        {
            return InteropFFTWF.dft_r2c_1d(length, input, output, InteropFFTWFlags.Estimate);
        }

        protected virtual void FreeUnmanagedMemory(IntPtr memoryBlock)
        {
            InteropFFTW.free(memoryBlock);
        }

        protected virtual void FreePlan(IntPtr fftPlan)
        {
            InteropFFTW.destroy_plan(fftPlan);
        }
    }
}
