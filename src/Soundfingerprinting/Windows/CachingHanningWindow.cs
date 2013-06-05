namespace Soundfingerprinting.Windows
{
    using System.Collections.Generic;

    public class CachingHanningWindow : IWindowFunction
    {
        private const int MaxElementsInCache = 10;

        private readonly IWindowFunction decorated;

        private readonly Dictionary<int, double[]> cache;

        public CachingHanningWindow(IWindowFunction decorated)
        {
            this.decorated = decorated;
            cache = new Dictionary<int, double[]>();
        }

        public void WindowInPlace(float[] outerspace, int windowLength)
        {
            decorated.WindowInPlace(outerspace, windowLength);
        }

        public double[] GetWindow(int length)
        {
            if (cache.ContainsKey(length))
            {
                return cache[length];
            }

            double[] window = decorated.GetWindow(length);
            
            if (cache.Count + 1 > MaxElementsInCache)
            {
                cache.Clear();
            }

            cache[length] = window;
            return window;
        }
    }
}