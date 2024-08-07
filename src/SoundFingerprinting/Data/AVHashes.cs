namespace SoundFingerprinting.Data
{
    using System;
    using ProtoBuf;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Content;

    /// <summary>
    ///  Container class that keeps either audio or video or both types of fingerprints generated from <see cref="AVTrack"/>.
    /// </summary>
    [Serializable]
    [ProtoContract(SkipConstructor = true)]
    public class AVHashes
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AVHashes"/> class.
        /// </summary>
        /// <param name="audio">Audio hashes (generated from  <see cref="AudioSamples"/>.</param>
        /// <param name="video">Video hashes (generated from <see cref="Frames"/>.</param>
        /// <exception cref="ArgumentException">Audio and video hashes can't be both null at the same time.</exception>
        public AVHashes(Hashes? audio, Hashes? video) : this(audio, video, AVFingerprintingTime.Zero())
        {
            // no op
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AVHashes"/> class.
        /// </summary>
        /// <param name="audio">Audio hashes (generated from  <see cref="AudioSamples"/>.</param>
        /// <param name="video">Video hashes (generated from <see cref="Frames"/>.</param>
        /// <param name="fingerprintingTime">Instance of <see cref="AVFingerprintingTime"/> fingerprinting times.</param>
        /// <exception cref="ArgumentException">Audio and video hashes can't be both null at the same time.</exception>
        public AVHashes(Hashes? audio, Hashes? video, AVFingerprintingTime fingerprintingTime)
        {
            if (audio == null && video == null)
            {
                throw new ArgumentException($"Both {nameof(audio)} and {nameof(video)} cannot be null at the same time.");
            }

            Audio = audio;
            Video = video;
            FingerprintingTime = fingerprintingTime;
        }

        /// <summary>
        ///  Gets audio hashes.
        /// </summary>
        [ProtoMember(1)]
        public Hashes? Audio { get; }
        
        /// <summary>
        ///  Gets video hashes.
        /// </summary>
        [ProtoMember(2)]
        public Hashes? Video { get; }

        /// <summary>
        ///  Gets fingerprinting statistics.
        /// </summary>
        [ProtoMember(3)]
        public AVFingerprintingTime FingerprintingTime { get; }

        /// <summary>
        ///  Gets total count of audio/video hashes.
        /// </summary>
        public int Count => (Audio?.Count ?? 0) + (Video?.Count ?? 0);

        /// <summary>
        ///  Gets relative to timestamp of the current <see cref="AVHashes"/> instance.
        /// </summary>
        public DateTime RelativeTo => Audio?.RelativeTo ?? Video?.RelativeTo ?? DateTime.MinValue;

        /// <summary>
        ///  Gets hashes stream id.
        /// </summary>
        public string? StreamId => Audio?.StreamId ?? Video?.StreamId;

        /// <summary>
        ///  Merges current instance of the AVHashes with provided one.
        /// </summary>
        /// <param name="next">Next instance of <see cref="AVHashes"/>.</param>
        /// <returns>A merged instance of <see cref="AVHashes"/> class.</returns>
        /// <remarks>
        ///  This method is useful when you would like to concatenate hashes that come from the realtime stream. <br />
        ///  Merging is done based on RelativeTo parameter of current and next.
        /// </remarks>
        public AVHashes MergeWith(AVHashes? next)
        {
            if (next == null)
            {
                return this;
            }
            
            var audio = Audio?.MergeWith(next.Audio) ?? next.Audio;
            var video = Video?.MergeWith(next.Video) ?? next.Video;
            var audioStats = FingerprintingTime.AudioMilliseconds + next.FingerprintingTime.AudioMilliseconds;
            var videoStats = FingerprintingTime.VideoMilliseconds + next.FingerprintingTime.VideoMilliseconds;
            return new AVHashes(audio, video, new AVFingerprintingTime(audioStats, videoStats));
        }

        /// <summary>
        ///  Gets a value indicating whether current instance of AVHashes contains hashes of either audio or video type.
        /// </summary>
        public bool IsEmpty => (Audio?.IsEmpty ?? true) && (Video?.IsEmpty ?? true);

        /// <summary>
        ///  Deconstructs current instance.
        /// </summary>
        /// <param name="audioHashes">Audio hashes.</param>
        /// <param name="videoHashes">Video hashes.</param>
        public void Deconstruct(out Hashes? audioHashes, out Hashes? videoHashes)
        {
            audioHashes = Audio;
            videoHashes = Video;
        }

        /// <summary>
        ///  Overrides current <see cref="RelativeTo"/> on Audio/Video hashes object.
        /// </summary>
        /// <param name="relativeTo">Relative to.</param>
        /// <returns>New instance of the <see cref="AVHashes"/> class.</returns>
        public AVHashes WithRelativeTo(DateTime relativeTo)
        {
            return new AVHashes(Audio?.WithRelativeTo(relativeTo), Video?.WithRelativeTo(relativeTo), FingerprintingTime);
        }

        /// <summary>
        ///  Adds a stream identifier to the Audio/Video hashes object.
        /// </summary>
        /// <param name="streamId">Stream ID.</param>
        /// <returns>New instance of the <see cref="AVHashes"/> class.</returns>
        public AVHashes WithStreamId(string streamId)
        {
            return new AVHashes(Audio?.WithStreamId(streamId), Video?.WithStreamId(streamId), FingerprintingTime);
        }

        /// <summary>
        ///  Add a property to the Audio/Video hashes object.
        /// </summary>
        /// <param name="key">Property key.</param>
        /// <param name="value">Property value.</param>
        /// <returns>New instance of the <see cref="AVHashes"/> class.</returns>
        public AVHashes WithProperty(string key, string value)
        {
            return new AVHashes(Audio?.WithProperty(key, value), Video?.WithProperty(key, value), FingerprintingTime);
        }
        
        /// <summary>
        ///  Gets a new instance of empty hashes.
        /// </summary>
        public static AVHashes Empty => new (Hashes.GetEmpty(MediaType.Audio), Hashes.GetEmpty(MediaType.Video), AVFingerprintingTime.Zero());

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"Audio:[{Audio}], Video:[{Video}]";
        }
    }
}