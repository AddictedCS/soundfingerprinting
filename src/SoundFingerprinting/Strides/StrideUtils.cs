namespace SoundFingerprinting.Strides
{
    using System;

    internal static class StrideUtils
    {
        public static IStride ToStride(string type, int maxStride, int minStride, int samplesPerFingerprint)
        {
            switch (type)
            {
                case "Static":
                    return new StaticStride(maxStride);
                case "Random":
                    return new RandomStride(minStride, maxStride);
                case "IncrementalStatic":
                    return new IncrementalStaticStride(maxStride, samplesPerFingerprint);
                case "IncrementalRandom":
                    return new IncrementalRandomStride(minStride, maxStride, samplesPerFingerprint);
                default:
                    throw new ArgumentException("Cannot find a matching type");
            }
        }
    }
}
