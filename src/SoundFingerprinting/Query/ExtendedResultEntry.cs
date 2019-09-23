
namespace SoundFingerprinting.Query
{
    using System;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.LCS;

    public class ExtendedResultEntry : ResultEntry
    {
        public ExtendedResultEntry(TrackData track, Coverage coverage, double confidence, double score, DateTime matchedAt)
            : base(track,
                coverage.QueryMatchStartsAt,
                coverage.QueryCoverageSeconds,
                coverage.MatchLengthWithTrackDiscontinuities,
                coverage.TrackMatchStartsAt,
                coverage.TrackStartsAt,
                confidence, 
                score, 
                coverage.QueryLength, 
                matchedAt)
        {
            ResultCoverage = coverage;
        }

        public Coverage ResultCoverage { get; }
    }
}
