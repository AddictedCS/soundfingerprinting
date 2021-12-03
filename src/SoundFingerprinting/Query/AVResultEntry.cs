namespace SoundFingerprinting.Query
{
    using System;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Class that contains audio/video result entry.
    /// </summary>
    public class AVResultEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AVResultEntry"/> class.
        /// </summary>
        /// <param name="audio">Audio result.</param>
        /// <param name="video">Video result.</param>
        /// <exception cref="ArgumentException">Argument exception in case both audio and video results are null.</exception>
        public AVResultEntry(ResultEntry? audio, ResultEntry? video)
        {
            if (audio == null && video == null)
            {
                throw new ArgumentException($"Both {nameof(audio)} and {nameof(video)} result entries cannot be null at the same time.");
            }

            Audio = audio;
            Video = video;
        }

        /// <summary>
        ///  Gets audio result entry.
        /// </summary>
        public ResultEntry? Audio { get; }

        /// <summary>
        ///  Gets video result entry.
        /// </summary>
        public ResultEntry? Video { get; }
        
        /// <summary>
        ///  Gets track info.
        /// </summary>
        /// <remarks>
        /// This abstraction is not exactly right. MediaType is inferred from Audio/Video result entries, which may not be returned from the server when one of media types does not match. <br/>
        /// See IHintStrategy which deals with issues related to review hints.
        /// </remarks>
        public TrackInfo TrackInfo => new TrackInfo(Track.Id, Track.Title, Track.Artist, Track.MetaFields, MediaType);
        
        /// <summary>
        ///  Gets matched at date.
        /// </summary>
        /// <remarks>
        ///  When both audio and video matches MatchedAt will be equal to minimum between audio and video matched date.
        /// </remarks>
        public DateTime MatchedAt
        {
            get
            {
                return (Audio, Video) switch
                {
                    (null, _) => Video!.MatchedAt,
                    (_, null) => Audio!.MatchedAt,
                    (_, _) => new DateTime(Math.Min(Audio!.MatchedAt.Ticks, Video!.MatchedAt.Ticks))
                };
            }
        }
 
        /// <summary>
        ///  Converts to audio video query match object that you can register in the registry service <see cref="IQueryMatchRegistry"/>.
        /// </summary>
        /// <param name="streamId">Stream identifier.</param>
        /// <param name="reviewStatus">review status.</param>
        /// <returns>An instance of <see cref="AVQueryMatch"/>.</returns>
        public AVQueryMatch ConvertToAvQueryMatch(string streamId = "", ReviewStatus reviewStatus = ReviewStatus.None)
        {
            return new AVQueryMatch(Guid.NewGuid().ToString(), streamId, ToQueryMatch(Audio), ToQueryMatch(Video), reviewStatus);
        }
        
        /// <summary>
        ///  Deconstruct <see cref="AVResultEntry"/>.
        /// </summary>
        /// <param name="audio">Audio result entry.</param>
        /// <param name="video">Video result entry.</param>
        public void Deconstruct(out ResultEntry? audio, out ResultEntry? video)
        {
            audio = Audio;
            video = Video;
        }

        private static QueryMatch? ToQueryMatch(ResultEntry? resultEntry)
        {
            if (resultEntry == null)
            {
                return null;
            }
            
            var track = new TrackInfo(resultEntry.Track.Id, resultEntry.Track.Title, resultEntry.Track.Artist, resultEntry.Track.MetaFields, resultEntry.Track.MediaType);
            return new QueryMatch(Guid.NewGuid().ToString(), track, resultEntry.Coverage, resultEntry.MatchedAt);
        }

        private TrackData Track => (Audio ?? Video)!.Track;
        
        private MediaType MediaType
        {
            get
            {
                return (Audio, Video) switch
                {
                    (_, null) => MediaType.Audio,
                    (null, _) => MediaType.Video,
                    (_, _) => MediaType.Audio | MediaType.Video
                };
            }
        } 
    }
}