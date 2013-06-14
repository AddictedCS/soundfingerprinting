namespace SoundFingerprinting.FFT.FFTW
{
    using System;
    using System.Collections.Generic;

    public class CachedFFTWService : FFTWService, IDisposable
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
            Dispose(true);
        }

        public override float[] FFTForward(float[] signal, int startIndex, int length)
        {
            lock (lockObject)
            {
                return fftwService.FFTForward(signal, startIndex, length);
            }
        }

        public void Dispose()
        {
            Dispose(false);
            alreadyDisposed = true;
            GC.SuppressFinalize(this);
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

        protected void Dispose(bool isDisposing)
        {
            if (!alreadyDisposed)
            {
                if (!isDisposing)
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
