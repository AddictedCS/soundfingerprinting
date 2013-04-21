using System;

namespace Soundfingerprinting.Fingerprinting.FFT.FFTW
{
    public class FFTWArray
    {
        public FFTWArray()
        {
            Input = IntPtr.Zero;
            Output = IntPtr.Zero;
            Plan = IntPtr.Zero;
        }

        public IntPtr Input { get; set; }

        public IntPtr Output { get; set; }

        public IntPtr Plan { get; set; }
    }
}
