namespace Soundfingerprinting.Fingerprinting.Windows
{
    using System.Linq;

    using Soundfingerprinting.Fingerprinting.FFT;

    public class NoWindow : IWindowFunction
    {
        public void WindowInPlace(float[] outerspace, int windowLength)
        {
            // empty
        }

        public void WindowInPlace(Complex[] outerspace, int windowLength)
        {
            // empty
        }

        public double[] GetWindow(int length)
        {
            return Enumerable.Repeat(1.0, length).ToArray();
        }
    }
}
