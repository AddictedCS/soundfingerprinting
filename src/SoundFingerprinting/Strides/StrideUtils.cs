namespace SoundFingerprinting.Strides
{
    using System;

    internal static class StrideUtils
    {
        private const int DefaultSamplesPerFingerprint = 128 * 64;
        
        public static IStride ToStride(string strideType, int min, int max)
        {
            switch (strideType)
            {
                case "Static":
                    return new IncrementalStaticStride(DefaultSamplesPerFingerprint + max);
                case "Random":
                    return new IncrementalRandomStride(DefaultSamplesPerFingerprint + min, DefaultSamplesPerFingerprint + max);
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
