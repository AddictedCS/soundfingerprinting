namespace SoundFingerprinting.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///  Frames class holding list of <see cref="Frame"/>.
    /// </summary>
    public class Frames : IEnumerable<Frame>
    {
        private readonly List<Frame> frames;

        /// <summary>
        /// Initializes a new instance of the <see cref="Frames"/> class.
        /// </summary>
        /// <param name="frames">List of frames.</param>
        /// <param name="origin">Origin identifier (i.e., filename).</param>
        /// <param name="frameRate">Frame rate (equivalent to fingerprint length).</param>
        /// <exception cref="ArgumentException">Instance of <see cref="ArgumentException"/> in case if <paramref name="frameRate"/> is zero.</exception>
        public Frames(IEnumerable<Frame> frames, string origin, int frameRate)
        {
            this.frames = frames.ToList();
            if (this.frames.Any() && frameRate == 0)
            {
                throw new ArgumentException(nameof(frameRate));
            }

            FrameRate = frameRate;
            Origin = origin;
            Duration = this.frames.Any() ? this.frames.Max(_ => _.StartsAt) + 1d / frameRate : 0;
            RelativeTo = DateTime.UtcNow.AddSeconds(-Duration);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Frames"/> class.
        /// </summary>
        /// <param name="frames">List of frames.</param>
        /// <param name="origin">Origin identifier (i.e., filename).</param>
        /// <param name="frameRate">Frame rate (equivalent to fingerprint length).</param>
        /// <param name="relativeTo">Relative to a particular date time.</param> 
        /// <exception cref="ArgumentException">Instance of <see cref="ArgumentException"/> in case if <paramref name="frameRate"/> is zero.</exception>
        public Frames(IEnumerable<Frame> frames, string origin, int frameRate, DateTime relativeTo)
        {
            this.frames = frames.ToList();
            if (this.frames.Any() && frameRate == 0)
            {
                throw new ArgumentException(nameof(frameRate));
            }

            FrameRate = frameRate;
            Origin = origin;
            Duration = this.frames.Any() ? this.frames.Max(_ => _.StartsAt) + 1d / frameRate : 0;
            RelativeTo = relativeTo;
        }

        /// <summary>
        ///  Gets relative to timestamp of the current <see cref="Frames"/> instance.
        /// </summary>
        public DateTime RelativeTo { get; }

        /// <summary>
        ///  Gets the duration in seconds of the current instance of <see cref="Frames"/> class.
        /// </summary>
        public double Duration { get; }

        /// <summary>
        ///  Gets the origin of the frames.
        /// </summary>
        public string Origin { get; }

        /// <summary>
        ///  Gets frame rate (equivalent to the fingerprint length).
        /// </summary>
        public int FrameRate { get; }

        /// <summary>
        ///  Gets enumerator of the frames.
        /// </summary>
        /// <returns>Instance of the <see cref="IEnumerable{Frame}"/> interface.</returns>
        public IEnumerator<Frame> GetEnumerator()
        {
            return frames.GetEnumerator();
        }

        /// <inheritdoc cref="GetEnumerator"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}