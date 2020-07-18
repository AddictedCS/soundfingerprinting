namespace SoundFingerprinting.Data
{
    using System;
    using ProtoBuf;

    [Serializable]
    [ProtoContract(SkipConstructor = true)]
    public class AVHashes
    {
        public AVHashes(Hashes audioHashes, Hashes videoHashes)
        {
            AudioHashes = audioHashes;
            VideoHashes = videoHashes;
        }

        [ProtoMember(1)]
        public Hashes AudioHashes { get; }
        
        [ProtoMember(2)]
        public Hashes VideoHashes { get; }
        
        public void Deconstruct(out Hashes audioHashes, out Hashes videoHashes)
        {
            audioHashes = AudioHashes;
            videoHashes = VideoHashes;
        }
        
        public static AVHashes Empty => new AVHashes(Hashes.Empty, Hashes.Empty);

        public override string ToString()
        {
            return $"Audio:[{AudioHashes}], Video:[{VideoHashes}]";
        }
    }
}