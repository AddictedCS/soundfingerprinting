namespace SoundFingerprinting.Content
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Data;

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
        public AVTrack(AudioTrack? audio, VideoTrack? video) : this(audio, video, (audio?.Duration ?? video?.Duration) ?? 0)
        {
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="AVTrack"/> class.
        /// </summary>
        /// <param name="audio">Audio track.</param>
        /// <param name="video">Video track.</param>
        /// <param name="totalEstimatedDuration">Total estimated duration of the original track.</param>
        /// <exception cref="ArgumentException">Both audio and video tracks can not be null.</exception>
        public AVTrack(AudioTrack? audio, VideoTrack? video, double totalEstimatedDuration)
        {
            if (audio == null && video == null)
            {
                throw new ArgumentException($"Both {nameof(audio)} and {nameof(video)} cannot be null at the same time.");
            }

            Audio = audio;
            Video = video;
            TotalEstimatedDuration = totalEstimatedDuration;
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
        ///  Gets total estimated duration of the original track that was provided as the source of the <see cref="AudioSamples"/> or <see cref="Frames"/>.
        /// </summary>
        /// <remarks>
        ///  This property is not equal to the length of the Audio or Video track. It is equal to the length of the original file from which the tracks have been read <br />
        ///  If you want to know the length of the underlying track, use audio <see cref="AudioTrack.Duration"/> or video <see cref="VideoTrack.Duration"/> properties.
        /// </remarks>
        public double TotalEstimatedDuration { get; }

        /// <summary>
        ///  Gets underlying <see cref="AudioSamples.RelativeTo"/> or <see cref="Frames.RelativeTo"/> of this audio/video track.
        /// </summary>
        public DateTime RelativeTo => Audio?.Samples.RelativeTo ?? Video!.Frames.RelativeTo;

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
            return new AVTrack(audio, video, TotalEstimatedDuration);
        }

        /// <summary>
        ///  Gets head of the original AVTrack.
        /// </summary>
        /// <param name="length">Length in seconds.</param>
        /// <returns>Region of the original AVTrack.</returns>
        public AVTrack Head(double length)
        {
            return new AVTrack(Audio?.Head(length), Video?.Head(length), TotalEstimatedDuration);
        }

        /// <summary>
        ///  Gets tail of the original AVTrack.
        /// </summary>
        /// <param name="start">Start at second.</param>
        /// <returns>Region of the original AVTrack.</returns>
        public AVTrack Tail(double start)
        {
            return new AVTrack(Audio?.Tail(start), Video?.Tail(start), TotalEstimatedDuration);
        }

        /// <summary>
        ///  Overrides current <see cref="AudioSamples.RelativeTo"/> and <see cref="Frames.RelativeTo"/> with a new one.
        /// </summary>
        /// <param name="relativeTo">New relative to property.</param>
        /// <returns>New instance of the  <see cref="AVTrack"/> class.</returns>
        public AVTrack WithRelativeTo(DateTime relativeTo)
        {
            var audioWithCaptureTime = Audio != null ? new AudioSamples(Audio.Samples.Samples, Audio.Samples.Origin, Audio.Samples.SampleRate, relativeTo) : null;
            var audio = audioWithCaptureTime != null ? new AudioTrack(audioWithCaptureTime) : null;
            var framesWithCaptureTime = Video != null ? new Frames(Video.Frames, Video.Frames.Origin, Video.Frames.FrameRate, relativeTo) : null;
            var video = framesWithCaptureTime != null ? new VideoTrack(framesWithCaptureTime) : null;
            return new AVTrack(audio, video, TotalEstimatedDuration);
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
