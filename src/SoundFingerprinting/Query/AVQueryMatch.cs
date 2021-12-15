namespace SoundFingerprinting.Query
{
    using System;
    using ProtoBuf;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LCS;

    /// <summary>
    ///  Class that holds all the information related to register audio video query match.
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class AVQueryMatch
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AVQueryMatch"/> class.
        /// </summary>
        /// <param name="id">Unique ID for a query match.</param>
        /// <param name="audio">Audio match information.</param>
        /// <param name="video">Video match information.</param>
        /// <param name="streamId">Stream ID information.</param>
        /// <param name="reviewStatus">Review status.</param>
        public AVQueryMatch(string id, QueryMatch? audio, QueryMatch? video, string? streamId, ReviewStatus reviewStatus = ReviewStatus.None)
        {
            if (audio == null && video == null)
            {
                throw new ArgumentException($"Both {nameof(audio)} and {nameof(video)} cannot be null at the same time");
            }
            
            Id = id;
            StreamId = streamId;
            Audio = audio;
            Video = video;
            ReviewStatus = reviewStatus;
        }

        /// <summary>
        ///  Gets unique ID for a query match. You can use this ID to search for query matches in Emy /api/v1/matches endpoint.
        /// </summary>
        [ProtoMember(1)]
        public string Id { get; }

        /// <summary>
        ///  Gets audio match information.
        /// </summary>
        /// <remarks>
        ///  Available when the performed query contained audio hashes.
        /// </remarks>
        [ProtoMember(2)] 
        public QueryMatch? Audio { get; }

        /// <summary>
        ///  Gets video match information.
        /// </summary>
        /// <remarks>
        ///  Available when the performed query contained video hashes.
        /// </remarks>
        [ProtoMember(3)] 
        public QueryMatch? Video { get; }

        /// <summary>
        ///  Gets stream ID information.
        /// </summary>
        /// <remarks>
        ///  Available when the performed query contained Hashes.StreamId field set.
        /// </remarks>
        [ProtoMember(4)]
        public string? StreamId { get; }
        
        /// <summary>
        ///  Gets review status.
        /// </summary>
        /// <remarks>
        ///  When the algorithm is not sure about the results, it will set this field with an appropriate flag that signals that human review is required.
        ///  Currently only set for Video matches that contain video artifacts.
        /// </remarks>
        [ProtoMember(5)] 
        public ReviewStatus ReviewStatus { get; }

        /// <summary>
        /// Gets track id match information.
        /// </summary>
        public string TrackId => (Audio ?? Video)!.Track.Id;
        
        /// <summary>
        ///  Gets relative date-time when the match occured.
        /// </summary>
        /// <remarks>
        ///  This field is calculated by adding Coverage.QueryMatchStartsAt to Hashes.RelativeTo, to identify exact position in time when the match occured.
        ///  The value represents minimum value between Audio and Video match.
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
        ///  Returns True if either of the matches Audio/Video passes track coverage threshold.
        ///  Returns False if current query match is considered a false positive, thus it can be safely discarded.
        /// </summary>
        /// <param name="relativeCoverageThreshold">Relative coverage in [0,1] range.</param>
        /// <exception cref="ArgumentException">If relative coverage threshold is not in [0,1] interval.</exception>
        /// <returns>Boolean true/false.</returns>
        public bool PassesCoverageThreshold(double relativeCoverageThreshold)
        {
            if (relativeCoverageThreshold is < 0 or > 1)
            {
                throw new ArgumentException($"{nameof(relativeCoverageThreshold)} should be between [0,1]");
            }

            return (Audio, Video) switch
            {
                (null, null) => false,
                (null, _) => TrackRelativeCoverage(Video!.Coverage) >= relativeCoverageThreshold,
                (_, null) => TrackRelativeCoverage(Audio!.Coverage) >= relativeCoverageThreshold,
                (_, _) => TrackRelativeCoverage(Audio!.Coverage) >= relativeCoverageThreshold || TrackRelativeCoverage(Video!.Coverage) >= relativeCoverageThreshold
            };
        }

        private static double TrackRelativeCoverage(Coverage coverage)
        {
            return coverage.TrackCoverageWithPermittedGapsLength / coverage.TrackLength;
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"Id=[{Id}],Audio=[{Audio}],Video=[{Video}]";
        }
    }
}