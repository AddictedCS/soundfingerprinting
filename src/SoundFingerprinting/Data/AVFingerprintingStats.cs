namespace SoundFingerprinting.Data
{
    using ProtoBuf;

    [ProtoContract(SkipConstructor = true)]
    public class AVFingerprintingStats
    {
        public AVFingerprintingStats(long audio, long video)
        {
            Audio = audio;
            Video = video;
        }

        [ProtoMember(1)]
        public long Audio { get; }

        [ProtoMember(2)]
        public long Video { get; }

        public static AVFingerprintingStats Add(AVFingerprintingStats? left, AVFingerprintingStats? right)
        {
            if (left == null || right == null)
            {
                return left ?? right ?? new AVFingerprintingStats(0, 0);
            }
            
            var audio = left.Audio + right.Audio;
            var video = left.Video + right.Video;
            return new AVFingerprintingStats(audio, video);
        }

        public static AVFingerprintingStats operator +(AVFingerprintingStats? left, AVFingerprintingStats? right)
        {
            return Add(left, right);
        }

        public static AVFingerprintingStats Zero()
        {
            return new AVFingerprintingStats(0, 0);
        }
    }
}