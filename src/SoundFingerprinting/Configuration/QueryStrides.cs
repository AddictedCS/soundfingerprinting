namespace SoundFingerprinting.Configuration
{
    using SoundFingerprinting.Strides;

    internal static class QueryStrides
    {
        public static IStride LowLatency = new IncrementalRandomStride(1536, 2048);
        public static IStride DefaultStride = new IncrementalRandomStride(768, 1024);
        public static IStride AggressiveStride = new IncrementalRandomStride(256, 512);
    }
}
