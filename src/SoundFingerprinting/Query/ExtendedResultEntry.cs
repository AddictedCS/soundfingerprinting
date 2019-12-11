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
            ResultCoverage = coverage;
        }

        public Coverage ResultCoverage { get; }

        public bool StrongMatch
        {
            get
            {
                bool trackGaps = ResultCoverage.TrackGaps.Any(g => !g.IsOnEdge);
                bool queryGaps = ResultCoverage.QueryGaps.Any(g => !g.IsOnEdge);
                double coverageWithPermittedGap = ResultCoverage.CoverageWithPermittedGapsLength;
                double trackLength = Track.Length;
                return coverageWithPermittedGap / trackLength >= 0.9 && !trackGaps && !queryGaps;
            }
        }

        public bool NoGaps => !ResultCoverage.TrackGaps.Any() && !ResultCoverage.QueryGaps.Any();
    }
}