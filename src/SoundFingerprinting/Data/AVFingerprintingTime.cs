namespace SoundFingerprinting.Data
{
    using System;
    using ProtoBuf;

    /// <summary>
    ///  Class that stores fingerprinting time information for <see cref="AVHashes"/>.
    /// </summary>
    [Serializable]
    [ProtoContract(SkipConstructor = true)]
    public class AVFingerprintingTime
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AVFingerprintingTime"/> class.
        /// </summary>
        /// <param name="audioMilliseconds">Audio fingerprinting time measured in milliseconds.</param>
        /// <param name="videoMilliseconds">Video fingerprinting time measured in milliseconds.</param>
        public AVFingerprintingTime(long audioMilliseconds, long videoMilliseconds)
        {
            AudioMilliseconds = audioMilliseconds;
            VideoMilliseconds = videoMilliseconds;
        }

        /// <summary>
        ///  Gets audio fingerprinting time in milliseconds.
        /// </summary>
        [ProtoMember(1)]
        public long AudioMilliseconds { get; }

        /// <summary>
        ///  Gets video fingerprinting time in milliseconds.
        /// </summary>
        [ProtoMember(2)]
        public long VideoMilliseconds { get; }

        /// <summary>
        ///  Add together fingerprinting times.
        /// </summary>
        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        /// <returns>Instance of the <see cref="AVFingerprintingTime"/> class with added times.</returns>
        public static AVFingerprintingTime Add(AVFingerprintingTime? left, AVFingerprintingTime? right)
        {
            if (left == null || right == null)
            {
                return left ?? right ?? Zero();
            }
            
            var audio = left.AudioMilliseconds + right.AudioMilliseconds;
            var video = left.VideoMilliseconds + right.VideoMilliseconds;
            return new AVFingerprintingTime(audio, video);
        }

        /// <summary>
        ///  Gets an instanceof of <see cref="AVFingerprintingTime"/> with zeroed audio and video fingerprinting time.
        /// </summary>
        /// <returns>Zeroed instance of the <see cref="AVFingerprintingTime"/> class.</returns>
        public static AVFingerprintingTime Zero()
        {
            return new AVFingerprintingTime(0, 0);
        }

        /// <summary>
        ///  Deconstructs audio/video fingerprinting time.
        /// </summary>
        /// <param name="audioMilliseconds">Audio milliseconds.</param>
        /// <param name="videoMilliseconds">Video milliseconds.</param>
        public void Deconstruct(out long audioMilliseconds, out long videoMilliseconds)
        {
            audioMilliseconds = AudioMilliseconds;
            videoMilliseconds = VideoMilliseconds;
        }
    }
}