namespace SoundFingerprinting.Strides
{
    using System;

    internal static class StrideUtils
    {
        public static IStride ToStride(string strideType, int min, int max)
        {
            switch (strideType)
            {
                case "Static":
                    return new StaticStride(max);
                case "Random":
                    return new RandomStride(min, max, 0);
                case "IncrementalStatic":
                    return new IncrementalStaticStride(max);
                case "IncrementalRandom":
                    return new IncrementalRandomStride(min, max);
                default:
                    throw new ArgumentException("Cannot find a matching strideType");
            }
        }
    }
}
