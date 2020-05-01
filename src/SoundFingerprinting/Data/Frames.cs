namespace SoundFingerprinting.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class Frames : IEnumerable<Frame>
    {
        private readonly List<Frame> frames;
        private readonly int frameRate;

        public Frames(IEnumerable<Frame> frames, string origin, int frameRate)
        {
            this.frames = frames.ToList();
            RelativeTo = DateTime.Now.AddSeconds((double) this.frames.Count / frameRate);
            this.frameRate = frameRate;
            Origin = origin;
        }

        public Frames(IEnumerable<Frame> frames, string origin, int frameRate, DateTime relativeTo)
        {
            RelativeTo = relativeTo;
            this.frames = frames.ToList();
            this.frameRate = frameRate;
            Origin = origin; 
        }
        
        public DateTime RelativeTo { get; }

        public double Duration => (double)frames.Count / frameRate;
        
        public string Origin { get; }

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