namespace Soundfingerprinting.Fingerprinting.FFT.FFTW
{
    using System;
    using System.Collections.Generic;

    public class CachedFFTWService : FFTWService, IDisposable
    {
        private readonly object lockObject = new object();
        
        private readonly Dictionary<int, FFTWArray> memory = new Dictionary<int, FFTWArray>(); // cache for in, out, plan arrays (in order not to allocate unmanaged memory on every call)

        private bool alreadyDisposed;

        ~CachedFFTWService()
        {
            Dispose(true);
        }

        public override float[] FFTForward(float[] signal, int startIndex, int length, double[] window)
        {
            lock (lockObject)
            {
                return base.FFTForward(signal, startIndex, length, window);
            }
        }

        public void Dispose()
        {
            Dispose(false);
            alreadyDisposed = true;
            GC.SuppressFinalize(this);
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
                    base.FreeUnmanagedMemory(item.Value.Input);
                    base.FreeUnmanagedMemory(item.Value.Output);
                    base.FreePlan(item.Value.Plan);
                }
            }
        }

        protected override IntPtr GetInput(int length)
        {
            if (memory.ContainsKey(length) && memory[length].Input != IntPtr.Zero)
            {
                return memory[length].Input;
            }

            IntPtr input = base.GetInput(length);
            SetKey(length, input, IntPtr.Zero, IntPtr.Zero);
            return input;
        }

        protected override IntPtr GetOutput(int length)
        {
            if (memory.ContainsKey(length) && memory[length].Output != IntPtr.Zero)
            {
                return memory[length].Output;
            }

            IntPtr output = base.GetOutput(length);
            SetKey(length, IntPtr.Zero, output, IntPtr.Zero);
            return output;
        }

        protected override void FreeUnmanagedMemory(IntPtr memoryBlock)
        {
            // do nothing
        }

        protected override void FreePlan(IntPtr fftPlan)
        {
            // do nothing    
        }

        protected override IntPtr GetFFTPlan(int length, IntPtr input, IntPtr output)
        {
            if (memory.ContainsKey(length) && memory[length].Plan != IntPtr.Zero)
            {
                return memory[length].Plan;
            }

            IntPtr plan = base.GetFFTPlan(length, input, output);
            SetKey(length, IntPtr.Zero, IntPtr.Zero, plan);
            return plan;
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
