namespace SoundFingerprinting.Strides
{
    using System;

    internal static class StrideUtils
    {
        public static IStride ToStride(string strideType, int min, int max, int samplesPerFingerprint)
        {
            switch (strideType)
            {
                case "Static":
                    return new StaticStride(max);
                case "Random":
                    return new RandomStride(min, max);
                case "IncrementalStatic":
                    return new IncrementalStaticStride(max, samplesPerFingerprint);
                case "IncrementalRandom":
                    return new IncrementalRandomStride(min, max, samplesPerFingerprint);
                default:
                    throw new ArgumentException("Cannot find a matching strideType");
            }
        }
    }
}
