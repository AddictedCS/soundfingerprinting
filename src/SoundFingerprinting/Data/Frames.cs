namespace SoundFingerprinting.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using ProtoBuf;

    [Serializable]
    [ProtoContract(IgnoreListHandling = true)]
    public class Frames : IEnumerable<Frame>
    {
        [ProtoMember(1)]
        private readonly IEnumerable<Frame> frames;

        public Frames(IEnumerable<Frame> frames, DateTime relativeTo, IEnumerable<string> origin)
        {
            RelativeTo = relativeTo;
            Origin = origin;
            this.frames = frames;
        }

        [ProtoMember(2)]
        public DateTime RelativeTo { get; }
        
        [ProtoMember(3)]
        public IEnumerable<string> Origin { get; }
        
        public IEnumerator<Frame> GetEnumerator()
        {
            return frames.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}