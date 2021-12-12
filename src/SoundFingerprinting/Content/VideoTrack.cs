namespace SoundFingerprinting.Content
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Data;
    using static System.Math;

    /// <summary>
    ///  Class container for video frames.
    /// </summary>
    public class VideoTrack
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoTrack"/> class.
        /// </summary>
        /// <param name="frames">Video frames.</param>
        public VideoTrack(Frames frames)
        {
            Frames = frames;
        }

        /// <summary>
        ///  Gets video frames.
        /// </summary>
        public Frames Frames { get; }

        /// <summary>
        ///  Gets frames duration.
        /// </summary>
        public double Duration => Frames.Duration;

        /// <summary>
        ///  Subtracts a part of the track according to start and length parameters.
        /// </summary>
        /// <param name="start">Start measured in seconds.</param>
        /// <param name="length">Length measured in seconds.</param>
        /// <returns>A new instance of the <see cref="VideoTrack"/> class.</returns>
        public VideoTrack SubTrack(double start, double length)
        {
            start = Max(start, 0);
            length = Min(length, Duration - start);
            int frameRate = Frames.FrameRate;
            int skippedFrames = (int)(start * frameRate);
            var subtracked = Frames
                .Skip(skippedFrames)
                .Take((int)(length * frameRate))
                .Select(f => new Frame(f.ImageRowCols, f.Rows, f.Cols, (float)(f.SequenceNumber - skippedFrames) / frameRate,
                    f.SequenceNumber - (uint)skippedFrames))
                .ToList();

            return new VideoTrack(new Frames(subtracked, Frames.Origin, Frames.FrameRate, Frames.RelativeTo.AddSeconds(start)));
        }

        /// <summary>
        ///  Cuts the video track from the beginning.
        /// </summary>
        /// <param name="length">Length measured in seconds to cut.</param>
        /// <returns>A new instance of the <see cref="VideoTrack"/> class.</returns>
        public VideoTrack Head(double length)
        {
            return SubTrack(0, length);
        }

        /// <summary>
        ///  Cuts the audio track from the end.
        /// </summary>
        /// <param name="start">Start measured in seconds to start at.</param>
        /// <returns>A new instance of the <see cref="VideoTrack"/> class.</returns>
        public VideoTrack Tail(double start)
        {
            return SubTrack(start, Duration - start);
        }

        /// <summary>
        ///  Concatenates multiple video tracks in one.
        /// </summary>
        /// <param name="tracks">Tracks to concatenate.</param>
        /// <returns>A new instance of the <see cref="VideoTrack"/> class.</returns>
        /// <exception cref="ArgumentException">Argument exception if the collection is empty.</exception>
        public static VideoTrack Concat(IReadOnlyCollection<VideoTrack> tracks)
        {
            if (!tracks.Any())
            {
                throw new ArgumentException("Cannot concat empty set.", nameof(tracks));
            }

            var first = tracks.First();
            var frameRate = first.Frames.FrameRate;
            var totalFrames = tracks.Sum(t => t.Frames.Count());
            var sequence = Enumerable.Range(0, totalFrames)
                .Select(i => new { startsAt = (float)i / frameRate, sequenceNumber = (uint)i, })
                .ToList();
            var frames = tracks
                .SelectMany(t => t.Frames)
                .Zip(sequence, (f, s) => new Frame(f.ImageRowCols, f.Rows, f.Cols, s.startsAt, s.sequenceNumber))
                .ToList();

            var relativeTo = tracks.Select(_ => _.Frames.RelativeTo).Min();
            return new VideoTrack(new Frames(frames, first.Frames.Origin, frameRate, relativeTo));
        }
    }
}