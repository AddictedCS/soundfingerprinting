﻿namespace SoundFingerprinting.Strides
{
    /// <summary>
    ///   Stride interface
    /// </summary>
    public interface IStride
    {
        /// <summary>
        ///   Gets the size of the first stride in samples
        /// </summary>
        int FirstStride { get; }

        /// <summary>
        ///   Gets next stride between 2 consecutive fingerprints (measured in audio samples)
        /// </summary>
        int NextStride { get; }
    }
}