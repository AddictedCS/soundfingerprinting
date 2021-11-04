namespace SoundFingerprinting.Data
{
    using System;
    using ProtoBuf;

    [Serializable]
    [ProtoContract(SkipConstructor = true)]
    public class AVHashes
    {
        public AVHashes(Hashes? audio, Hashes? video, AVFingerprintingStats stats)
        {
            if (audio == null && video == null)
            {
                throw new ArgumentException($"Both {nameof(audio)} and {nameof(video)} cannot be null at the same time.");
            }
            
            Audio = audio;
            Video = video;
            Stats = stats;
        }

        [ProtoMember(1)]
        public Hashes? Audio { get; }
        
        [ProtoMember(2)]
        public Hashes? Video { get; }

        [ProtoMember(3)]
        public AVFingerprintingStats Stats { get; }

        public DateTime CaptureTime => Audio?.RelativeTo ?? Video?.RelativeTo ?? DateTime.MinValue;

        public AVHashes MergeWith(AVHashes? next)
        {
            if (next == null)
            {
                return this;
            }
            
            var audio = Audio?.MergeWith(next.Audio) ?? next.Audio;
            var video = Video?.MergeWith(next.Video) ?? next.Video;
            var audioStats = Stats.Audio + next.Stats.Audio;
            var videoStats = Stats.Video + next.Stats.Video;
            return new AVHashes(audio, video, new AVFingerprintingStats(audioStats, videoStats));
        }

        public bool IsEmpty => (Audio?.IsEmpty ?? true) && (Video?.IsEmpty ?? true);

        public void Deconstruct(out Hashes? audioHashes, out Hashes? videoHashes)
        {
            audioHashes = Audio;
            videoHashes = Video;
        }
        
        public static AVHashes Empty => new AVHashes(Hashes.GetEmpty(MediaType.Audio), Hashes.GetEmpty(MediaType.Video), AVFingerprintingStats.Zero());

        public override string ToString()
        {
            return $"Audio:[{Audio}], Video:[{Video}]";
        }
    }
}