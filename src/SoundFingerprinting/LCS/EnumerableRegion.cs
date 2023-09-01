namespace SoundFingerprinting.LCS
{
    internal record EnumerableRegion(int StartAt, int EndAt)
    {
        /// <summary>
        ///  Gets start at index.
        /// </summary>
        public int StartAt { get; } = StartAt;

        /// <summary>
        ///  Gets ends at index.
        /// </summary>
        public int EndAt { get; } = EndAt;

        /// <summary>
        ///  Gets number of elements between start and end.
        /// </summary>
        public int Count => EndAt - StartAt + 1;
    }
}