namespace SoundFingerprinting.Content
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///  Data class that holds an instance of audio or video tracks.
    /// </summary>
    public class AVTrack
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="AVTrack"/> class.
        /// </summary>
        /// <param name="audio">Audio track.</param>
        /// <param name="video">Video track.</param>
        /// <exception cref="ArgumentException">Both audio and video tracks can not be null.</exception>
        public AVTrack(AudioTrack? audio, VideoTrack? video)
        {
            if (audio == null && video == null)
            {
                throw new ArgumentException($"Both {nameof(audio)} and {nameof(video)} cannot be null at the same time.");
            }

            Audio = audio;
            Video = video;
        }

        /// <summary>
        ///  Gets audio track.
        /// </summary>
        public AudioTrack? Audio { get; }

        /// <summary>
        ///  Gets video track.
        /// </summary>
        public VideoTrack? Video { get; }

        /// <summary>
        /// Gets a portion of the original AVTrack track.
        /// </summary>
        /// <param name="start">Starts at second.</param>
        /// <param name="length">Length to subtract.</param>
        /// <returns>Region of the original AVTrack.</returns>
        public AVTrack SubTrack(double start, double length)
        {
            var audio = Audio?.SubTrack(start, length);
            var video = Video?.SubTrack(start, length);
            return new AVTrack(audio, video);
        }

        /// <summary>
        ///  Gets head of the original AVTrack.
        /// </summary>
        /// <param name="length">Length in seconds.</param>
        /// <returns>Region of the original AVTrack.</returns>
        public AVTrack Head(double length)
        {
            return new AVTrack(Audio?.Head(length), Video?.Head(length));
        }

        /// <summary>
        ///  Gets tail of the original AVTrack.
        /// </summary>
        /// <param name="start">Start at second.</param>
        /// <returns>Region of the original AVTrack.</returns>
        public AVTrack Tail(double start)
        {
            return new AVTrack(Audio?.Tail(start), Video?.Tail(start));
        }

        /// <summary>
        ///  Deconstructs given instance of the <see cref="AVTrack"/> class.
        /// </summary>
        /// <param name="audio">Audio track.</param>
        /// <param name="video">Video track.</param>
        public void Deconstruct(out AudioTrack? audio, out VideoTrack? video)
        {
            audio = Audio;
            video = Video;
        }

        /// <summary>
        ///  Concatenates given collection of audio/video tracks.
        /// </summary>
        /// <param name="tracks">Tracks to concatenate.</param>
        /// <returns>Concatenated instance of the <see cref="AVTrack"/> class.</returns>
        /// <exception cref="ArgumentException">Argument exception if the collection is empty.</exception>
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

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"AVTrack[audio={Audio?.Duration ?? 0:00},video={Video?.Duration ?? 0:00}]";
        }
    }
}
