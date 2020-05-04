namespace SoundFingerprinting.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class Frames : IEnumerable<Frame>
    {
        private readonly List<Frame> frames;

        public Frames(IEnumerable<Frame> frames, string origin, int frameRate)
        {
            this.frames = frames.ToList();
            if (this.frames.Any() && frameRate == 0)
            {
                throw new ArgumentException(nameof(frameRate));
            }

            RelativeTo = DateTime.Now.AddSeconds((double) this.frames.Count / frameRate);
            FrameRate = frameRate;
            Origin = origin;
        }

        public Frames(IEnumerable<Frame> frames, string origin, int frameRate, DateTime relativeTo)
        {
            this.frames = frames.ToList();
            if (this.frames.Any() && frameRate == 0)
            {
                throw new ArgumentException(nameof(frameRate));
            }

            RelativeTo = relativeTo;
            FrameRate = frameRate;
            Origin = origin;
        }

        public DateTime RelativeTo { get; }

        public double Duration => (double) frames.Count / FrameRate;

        public string Origin { get; }

        public int FrameRate { get; }

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