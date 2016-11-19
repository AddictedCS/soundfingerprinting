﻿namespace SoundFingerprinting.FFT.FFTW
{
    using System;
    using System.Runtime.InteropServices;

    using SoundFingerprinting.FFT.FFTW.X64;

    public class FFTWService64 : FFTWService
    {
        public override float[] FFTForward(float[] data, int startIndex, int length, float[] window)
        {
            IntPtr input = GetInput(length);
            IntPtr output = GetOutput(length);
            IntPtr fftPlan = GetFFTPlan(length, input, output);
            float[] applyTo = new float[length];
            Array.Copy(data, startIndex, applyTo, 0, length);
            Window(applyTo, window);
            Marshal.Copy(applyTo, 0, input, length);
            FFTWNativeMethods.execute(fftPlan);
            float[] result = new float[length * 2];
            Marshal.Copy(output, result, 0, length);
            FreeUnmanagedMemory(input);
            FreeUnmanagedMemory(output);
            FreePlan(fftPlan);
            return result;
        }

        public override IntPtr GetOutput(int length)
        {
            return FFTWNativeMethods.malloc(8 * length);
        }

        public override IntPtr GetInput(int length)
        {
            return FFTWNativeMethods.malloc(4 * length);
        }

        public override IntPtr GetFFTPlan(int length, IntPtr input, IntPtr output)
        {
            return FFTWFNativeMethods.dft_r2c_1d(length, input, output, InteropFFTWFlags.Estimate);
        }

        public override void FreeUnmanagedMemory(IntPtr memoryBlock)
        {
            FFTWNativeMethods.free(memoryBlock);
        }

        public override void FreePlan(IntPtr fftPlan)
        {
            FFTWNativeMethods.destroy_plan(fftPlan);
        }

        public override void Execute(IntPtr fftPlan)
        {
            FFTWNativeMethods.execute(fftPlan);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // release managed resources
            }

            // release unmanaged resources
        }
    }
}
