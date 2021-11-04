namespace SoundFingerprinting.Content
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AVTrack
    {
        public AVTrack(AudioTrack? audio, VideoTrack? video)
        {
            if (audio == null && video == null)
            {
                throw new ArgumentException($"Both {nameof(audio)} and {nameof(video)} cannot be null at the same time.");
            }

            Audio = audio;
            Video = video;
        }

        public AudioTrack? Audio { get; }

        public VideoTrack? Video { get; }

        public AVTrack SubTrack(double start, double length)
        {
            var audio = Audio?.SubTrack(start, length);
            var video = Video?.SubTrack(start, length);

            return new AVTrack(audio, video);
        }

        public AVTrack Head(double length)
        {
            return new AVTrack(Audio?.Head(length), Video?.Head(length));
        }

        public AVTrack Tail(double start)
        {
            return new AVTrack(Audio?.Tail(start), Video?.Tail(start));
        }

        public void Deconstruct(out AudioTrack? audio, out VideoTrack? video)
        {
            audio = Audio;
            video = Video;
        }

        public static AVTrack Concat(IReadOnlyCollection<AVTrack> tracks)
        {
            if (!tracks.Any())
            {
                throw new ArgumentException("Cannot concat empty set.", nameof(tracks));
            }

            var audioTracks = tracks.Select(t => t.Audio).Where(t => t != null).Select(t => t!).ToList();
            var videoTracks = tracks.Select(t => t.Video).Where(t => t != null).Select(t => t!).ToList();

            var audio = audioTracks.Any() ? AudioTrack.Concat(audioTracks) : null;
            var video = videoTracks.Any() ? VideoTrack.Concat(videoTracks) : null;

            return new AVTrack(audio, video);
        }
    }
}
