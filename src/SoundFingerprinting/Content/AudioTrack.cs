namespace SoundFingerprinting.Content
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Data;
    using static System.Math;

    public class AudioTrack : Track
    {
        public AudioTrack(AudioSamples samples, double totalEstimatedDuration)
        {
            Samples = samples;
            TotalEstimatedDuration = totalEstimatedDuration;
        }

        public AudioSamples Samples { get; }

        public override MediaType Type => MediaType.Audio;

        public override double Duration => Samples.Duration;

        public override double TotalEstimatedDuration { get; }

        public AudioTrack SubTrack(double start, double length)
        {
            start = Max(start, 0);
            length = Min(length, Duration - start);
            var samples = Samples.Samples
                .Skip((int)(start * Samples.SampleRate))
                .Take((int)(length * Samples.SampleRate))
                .ToArray();
            return new AudioTrack(new AudioSamples(samples, Samples.Origin, Samples.SampleRate, Samples.RelativeTo), TotalEstimatedDuration);
        }

        public AudioTrack Head(double length)
        {
            return SubTrack(0, length);
        }

        public AudioTrack Tail(double start)
        {
            return SubTrack(start, Duration - start);
        }

        public static AudioTrack Concat(IReadOnlyCollection<AudioTrack> tracks)
        {
            if (!tracks.Any())
            {
                throw new ArgumentException("Cannot concat empty set.", nameof(tracks));
            }

            var audioTrack = tracks.First();
            var first = audioTrack.Samples;
            var samples = tracks
                .SelectMany(t => t.Samples.Samples)
                .ToArray();
            var audioSamples = new AudioSamples(samples, first.Origin, first.SampleRate, first.RelativeTo);
            return new AudioTrack(audioSamples, audioTrack.TotalEstimatedDuration);
        }
    }
}
