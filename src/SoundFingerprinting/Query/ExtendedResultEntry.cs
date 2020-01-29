namespace SoundFingerprinting.Query
{
    using System;
    using System.Linq;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.LCS;

    public class ExtendedResultEntry : ResultEntry
    {
        public ExtendedResultEntry(TrackData track, Coverage coverage, double confidence, double score, DateTime matchedAt)
            : base(track,
                coverage.QueryMatchStartsAt,
                coverage.CoverageLength,
                coverage.DiscreteCoverageLength,
                coverage.TrackMatchStartsAt,
                coverage.TrackStartsAt,
                confidence,
                score,
                coverage.QueryLength,
                matchedAt)
        {
            Coverage = coverage;
        }

        public Coverage Coverage { get; }

        public bool NoGaps => !Coverage.TrackGaps.Any() && !Coverage.QueryGaps.Any();
    }
}