namespace SoundFingerprinting.FFT.FFTW
{
    using System;
    using System.Runtime.InteropServices;

    using SoundFingerprinting.FFT.FFTW.X64;

    public class FFTWService64 : FFTWService
    {
        public override float[] FFTForward(float[] signal, int startIndex, int length)
        {
            IntPtr input = GetInput(length);
            IntPtr output = GetOutput(length);
            IntPtr fftPlan = GetFFTPlan(length, input, output);
            float[] applyTo = new float[length];
            Array.Copy(signal, startIndex, applyTo, 0, length);
            Marshal.Copy(applyTo, 0, input, length);
            InteropFFTW.execute(fftPlan);
            float[] result = new float[length * 2];
            Marshal.Copy(output, result, 0, length);
            FreeUnmanagedMemory(input);
            FreeUnmanagedMemory(output);
            FreePlan(fftPlan);
            return result;
        }

        public override IntPtr GetOutput(int length)
        {
            return InteropFFTW.malloc(8 * length);
        }

        public override IntPtr GetInput(int length)
        {
            return InteropFFTW.malloc(4 * length);
        }

        public override IntPtr GetFFTPlan(int length, IntPtr input, IntPtr output)
        {
            return InteropFFTWF.dft_r2c_1d(length, input, output, InteropFFTWFlags.Estimate);
        }

        public override void FreeUnmanagedMemory(IntPtr memoryBlock)
        {
            InteropFFTW.free(memoryBlock);
        }

        public override void FreePlan(IntPtr fftPlan)
        {
            InteropFFTW.destroy_plan(fftPlan);
        }
    }
}
