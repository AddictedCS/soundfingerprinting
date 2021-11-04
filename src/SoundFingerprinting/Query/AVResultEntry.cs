namespace SoundFingerprinting.Query
{
    using System;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public class AVResultEntry
    {
        public AVResultEntry(ResultEntry? audio, ResultEntry? video)
        {
            if (audio == null && video == null)
            {
                throw new ArgumentException($"Both {nameof(audio)} and {nameof(video)} result entries cannot be null at the same time.");
            }

            Audio = audio;
            Video = video;
        }

        public ResultEntry? Audio { get; }

        public ResultEntry? Video { get; }
        
        // TODO this abstraction is not exactly right
        // MediaType is inferred from Audio/Video result entries, which may not be returned from the server due to various problems
        // see IHintStrategy which deals with issues related to review hints
        public TrackInfo TrackInfo => new TrackInfo(Track.Id, Track.Title, Track.Artist, Track.MetaFields, MediaType);
 
        public AVQueryMatch ConvertToAvQueryMatch(string streamId = "", ReviewStatus reviewStatus = ReviewStatus.None)
        {
            return new AVQueryMatch(Guid.NewGuid().ToString(), streamId, ToQueryMatch(Audio), ToQueryMatch(Video), reviewStatus);
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

        public TrackData Track => (Audio ?? Video)!.Track;

        public DateTime MatchDate
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
        
        public void Deconstruct(out ResultEntry? audio, out ResultEntry? video)
        {
            audio = Audio;
            video = Video;
        }
    }
}