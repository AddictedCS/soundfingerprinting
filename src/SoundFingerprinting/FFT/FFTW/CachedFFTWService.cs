namespace SoundFingerprinting.FFT.FFTW
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal class CachedFFTWService : FFTWService
    {
        private readonly FFTWService fftwService;

        private readonly object lockObject = new object();
        
        private readonly Dictionary<int, FFTWArray> memory = new Dictionary<int, FFTWArray>(); // cache for in, out, plan arrays (in order not to allocate unmanaged memory on every call)

        private bool alreadyDisposed;

        public CachedFFTWService(FFTWService fftwService)
        {
            this.fftwService = fftwService;
        }

        ~CachedFFTWService()
        {
            Dispose(false);
        }

        public override float[] FFTForward(float[] signal, int startIndex, int length)
        {
            lock (lockObject)
            {
                IntPtr input = GetInput(length);
                IntPtr output = GetOutput(length);
                IntPtr fftPlan = GetFFTPlan(length, input, output);
                float[] applyTo = new float[length];
                Array.Copy(signal, startIndex, applyTo, 0, length);
                Marshal.Copy(applyTo, 0, input, length);
                Execute(fftPlan);
                float[] result = new float[length * 2];
                Marshal.Copy(output, result, 0, length);
                FreeUnmanagedMemory(input);
                FreeUnmanagedMemory(output);
                FreePlan(fftPlan);
                return result;
            }
        }

        public override IntPtr GetInput(int length)
        {
            if (memory.ContainsKey(length) && memory[length].Input != IntPtr.Zero)
            {
                return memory[length].Input;
            }

            IntPtr input = fftwService.GetInput(length);
            SetKey(length, input, IntPtr.Zero, IntPtr.Zero);
            return input;
        }

        public override IntPtr GetOutput(int length)
        {
            if (memory.ContainsKey(length) && memory[length].Output != IntPtr.Zero)
            {
                return memory[length].Output;
            }

            IntPtr output = fftwService.GetOutput(length);
            SetKey(length, IntPtr.Zero, output, IntPtr.Zero);
            return output;
        }

        public override void FreeUnmanagedMemory(IntPtr memoryBlock)
        {
            // do nothing
        }

        public override void FreePlan(IntPtr fftPlan)
        {
            // do nothing    
        }

        public override void Execute(IntPtr fftPlan)
        {
            fftwService.Execute(fftPlan);
        }

        public override IntPtr GetFFTPlan(int length, IntPtr input, IntPtr output)
        {
            if (memory.ContainsKey(length) && memory[length].Plan != IntPtr.Zero)
            {
                return memory[length].Plan;
            }

            IntPtr plan = fftwService.GetFFTPlan(length, input, output);
            SetKey(length, IntPtr.Zero, IntPtr.Zero, plan);
            return plan;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (!alreadyDisposed)
            {
                alreadyDisposed = true;
                if (isDisposing)
                {
                    // release managed resources
                }

                foreach (var item in memory)
                {
                    fftwService.FreeUnmanagedMemory(item.Value.Input);
                    fftwService.FreeUnmanagedMemory(item.Value.Output);
                    fftwService.FreePlan(item.Value.Plan);
                }
            }
        }

        private void SetKey(int length, IntPtr input, IntPtr output, IntPtr fftPlan)
        {
            if (!memory.ContainsKey(length))
            {
                memory[length] = new FFTWArray { Input = input, Output = output, Plan = fftPlan };
                return;
            }

            if (input != IntPtr.Zero)
            {
                memory[length].Input = input;
            }

            if (output != IntPtr.Zero)
            {
                memory[length].Output = output;
            }

            if (fftPlan != IntPtr.Zero)
            {
                memory[length].Plan = fftPlan;
            }
        }
    }
}
