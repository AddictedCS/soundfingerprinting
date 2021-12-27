namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Strides;

    public static class Configs
    {
        public static class FrequencyRanges
        {
            public static FrequencyRange Default => new FrequencyRange(318, 2000);
        }

        public static class Threshold
        {
            public static int Default = 4;
        }

        public static class FingerprintStrides
        {
            public static IStride Default => new IncrementalStaticStride(512);
        }

        public static class QueryStrides
        {
            public static IStride DefaultStride => new IncrementalRandomStride(256, 512);
        }
    }
}
