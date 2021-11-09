namespace SoundFingerprinting.Content
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Data;
    using static System.Math;

    public class VideoTrack : Track
    {
        public Frames Frames { get; }

        public VideoTrack(Frames frames, double totalEstimatedDuration)
        {
            Frames = frames;
            TotalEstimatedDuration = totalEstimatedDuration;
        }

        public override MediaType Type => MediaType.Video;

        public override double Duration => Frames.Duration;
        
        public override double TotalEstimatedDuration { get; }

        public VideoTrack SubTrack(double start, double length)
        {
            start = Max(start, 0);
            length = Min(length, Duration - start);
            int frameRate = Frames.FrameRate;
            int skippedFrames = (int)(start * frameRate);
            var subtracked = Frames
                .Skip(skippedFrames)
                .Take((int)(length * frameRate))
                .Select(f => new Frame(f.ImageRowCols, f.Rows, f.Cols, (float)(f.SequenceNumber - skippedFrames) / frameRate, f.SequenceNumber - (uint)skippedFrames))
                .ToList();
            
            return new VideoTrack(new Frames(subtracked, Frames.Origin, Frames.FrameRate, Frames.RelativeTo.AddSeconds(start)), TotalEstimatedDuration);
        }

        public VideoTrack Head(double length)
        {
            return SubTrack(0, length);
        }

        public VideoTrack Tail(double start)
        {
            return SubTrack(start, Duration - start);
        }

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
                .Select(i => new { startsAt = (float)i / frameRate, sequenceNumber = (uint)i,  })
                .ToList();
            var frames = tracks
                .SelectMany(t => t.Frames)
                .Zip(sequence, (f, s) => new Frame(f.ImageRowCols, f.Rows, f.Cols, s.startsAt, s.sequenceNumber))
                .ToList();

            var relativeTo = tracks.Select(_ => _.Frames.RelativeTo).Min();
            return new VideoTrack(new Frames(frames, first.Frames.Origin, frameRate, relativeTo), first.TotalEstimatedDuration);
        }
    }
}
