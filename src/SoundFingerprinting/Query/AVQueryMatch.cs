namespace SoundFingerprinting.Query
{
    using System;
    using ProtoBuf;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LCS;

    [ProtoContract(SkipConstructor = true)]
    public class AVQueryMatch
    {
        public AVQueryMatch(string id, string streamId, QueryMatch? audio, QueryMatch? video, ReviewStatus reviewStatus = ReviewStatus.None)
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

        [ProtoMember(1)]
        public string Id { get; }

        [ProtoMember(2)] 
        public QueryMatch? Audio { get; }

        [ProtoMember(3)] 
        public QueryMatch? Video { get; }

        [ProtoMember(4)]
        public string StreamId { get; }
        
        [ProtoMember(5)] 
        public ReviewStatus ReviewStatus { get; }

        public string TrackId => (Audio ?? Video)!.Track.Id;
        
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
        
        /// <summary>
        ///  Returns True if either of the matches Audio/Video passes track coverage threshold.
        ///  Returns False if current query match is considered a false positive, thus it can be safely discarded.
        /// </summary>
        /// <param name="relativeCoverageThreshold">Relative coverage in [0,1] range.</param>
        /// <exception cref="ArgumentException">If relative coverage threshold is not in [0,1] interval.</exception>
        /// <returns>Boolean true/false.</returns>
        public bool PassesCoverageThreshold(double relativeCoverageThreshold)
        {
            if (relativeCoverageThreshold < 0 || relativeCoverageThreshold > 1)
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

        public override string ToString()
        {
            return $"Id=[{Id}],Audio=[{Audio}],Video=[{Video}]";
        }
    }
}