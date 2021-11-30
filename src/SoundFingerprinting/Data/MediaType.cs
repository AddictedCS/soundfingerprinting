namespace SoundFingerprinting.Data
{
    using System;

    /// <summary>
    ///  Track media type.
    /// </summary>
    [Flags]
    public enum MediaType
    {
        /// <summary>
        ///  Audio track media type.
        /// </summary>
        Audio = 1,
        
        /// <summary>
        /// Video track media type.
        /// </summary>
        Video = 2
    }
}