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
        ///  Gets track id.
        /// </summary>
        public string TrackId => TrackData.Id;

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
        /// <param name="avQueryMatchId">Query match identifier.</param>
        /// <param name="streamId">Stream identifier.</param>
        /// <param name="reviewStatus">review status.</param>
        /// <returns>An instance of <see cref="AVQueryMatch"/>.</returns>
        public AVQueryMatch ConvertToAvQueryMatch(string avQueryMatchId = "", string streamId = "", ReviewStatus reviewStatus = ReviewStatus.None)
        {
            string id = string.IsNullOrEmpty(avQueryMatchId) ? Guid.NewGuid().ToString() : avQueryMatchId;
            return new AVQueryMatch(id, ToQueryMatch(Audio), ToQueryMatch(Video), streamId, reviewStatus);
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

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"AVResultEntry[Audio={Audio},Video={Video}]";
        }

        /// <summary>
        ///  Calculates the overlap between two entries.
        /// </summary>
        /// <param name="entry">Entry to check on.</param>
        /// <returns>Overlap measured in seconds (0d if there is no overlap).</returns>
        /// <remarks>
        ///  This method is symmetric, if this overlaps entry, then entry overlaps this.
        /// </remarks>
        public double GetOverlap(AVResultEntry entry)
        {
            var (left, right) = MatchedAt < entry.MatchedAt ? (this, entry) : (entry, this);

            var leftEnd = left.MatchedAt.AddSeconds((left.Audio?.Coverage.TrackDiscreteCoverageLength ?? left.Video?.Coverage.TrackDiscreteCoverageLength) ?? 0);
            var rightEndsAt = right.MatchedAt.AddSeconds((right.Audio?.Coverage.TrackDiscreteCoverageLength ?? right.Video?.Coverage.TrackDiscreteCoverageLength) ?? 0);

            if (leftEnd < right.MatchedAt)
            {
                // left ends before right starts
                return 0d;
            }

            // there is an overlap
            if (rightEndsAt < leftEnd)
            {
                // right is contained within left
                return (right.Audio?.Coverage.TrackDiscreteCoverageLength ?? right.Video?.Coverage.TrackDiscreteCoverageLength) ?? 0;
            }

            return (leftEnd - right.MatchedAt).TotalSeconds;
        }
        
        /// <summary>
        ///  Audio/Video result entries are equivalent if they have the same track id and matched at date.
        /// </summary>
        /// <param name="entry">Entry to compare to.</param>
        /// <returns>True if equivalent, otherwise false.</returns>
        public bool IsEquivalent(AVResultEntry entry)
        {
            return TrackId == entry.TrackId && MatchedAt == entry.MatchedAt;
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

        private TrackData TrackData => (Audio ?? Video)!.Track;
    }
}