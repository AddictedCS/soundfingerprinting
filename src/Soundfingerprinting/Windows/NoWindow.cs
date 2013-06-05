namespace Soundfingerprinting.Windows
{
    using System.Linq;

    public class NoWindow : IWindowFunction
    {
        public void WindowInPlace(float[] outerspace, int windowLength)
        {
            // empty
        }

        public double[] GetWindow(int length)
        {
            return Enumerable.Repeat(1.0, length).ToArray();
        }
    }
}
