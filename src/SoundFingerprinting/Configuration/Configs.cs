﻿namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Strides;

    public static class Configs
    {
        public static class FrequencyRanges
        {
            public static FrequencyRange LowLatency => new FrequencyRange(318, 2000);

            public static FrequencyRange Default => new FrequencyRange(318, 2000);
            public static FrequencyRange HighPrecision => new FrequencyRange(1200, 2500);
        }

        public static class Threshold
        {
            public static int LowLatency = 5;
            public static int Default = 4;
            public static int HighPrecision = 3;
        }

        public static class FingerprintStrides
        {
            public static IStride LowLatency => new IncrementalStaticStride(1536);
            public static IStride Default => new IncrementalStaticStride(512);
            public static IStride HighPrecision => new IncrementalStaticStride(512);
        }

        public static class QueryStrides
        {
            public static IStride LowLatency => new IncrementalRandomStride(512, 768);
            public static IStride DefaultStride => new IncrementalRandomStride(256, 512);
            public static IStride HighPrecisionStride => new IncrementalRandomStride(256, 512);
        }
    }
}
