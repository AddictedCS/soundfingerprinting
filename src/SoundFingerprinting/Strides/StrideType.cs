namespace SoundFingerprinting.Strides
{
    /// <summary>
    ///   Types of strides
    /// </summary>
    internal enum StrideType
    {
        /// <summary>
        ///   Static stride between 2 consecutive fingerprints
        /// </summary>
        Static,

        /// <summary>
        ///   Random stride between 2 consecutive fingerprints
        /// </summary>
        Random,

        /// <summary>
        ///   Incremental static stride between start of 2 consecutive fingerprints
        /// </summary>
        IncrementalStatic,

        /// <summary>
        ///   Incremental random stride between start of 2 consecutive fingerprints
        /// </summary>
        IncrementalRandom
    }
}