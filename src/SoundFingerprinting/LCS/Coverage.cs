namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;

    using SoundFingerprinting.Query;

    public class Coverage
    {
        public Coverage(double queryMatchStartsAt, double queryMatchLength, double queryCoverageLength, double trackMatchStartsAt, double trackStartsAt, double queryLength, double scoreAvgAcrossCoverage, int sourceMatchesCountAcrossCoverage, IEnumerable<MatchedWith> bestPath)
        {
            QueryMatchStartsAt = queryMatchStartsAt;
            QueryMatchLength = queryMatchLength;
            TrackMatchStartsAt = trackMatchStartsAt;
            TrackStartsAt = trackStartsAt;
            QueryCoverageLength = queryCoverageLength;
            QueryLength = queryLength;
            ScoreAvgAcrossCoverage = scoreAvgAcrossCoverage;
            SourceMatchesCountAcrossCoverage = sourceMatchesCountAcrossCoverage;
            BestPath = bestPath;
        }

        public double QueryMatchStartsAt { get; }

        public double QueryMatchLength { get; }

        public double QueryCoverageLength { get; }

        public double TrackMatchStartsAt { get; }

        public double TrackStartsAt { get; }

        public double QueryLength { get; }

        public double ScoreAvgAcrossCoverage { get; }

        public int SourceMatchesCountAcrossCoverage { get; }

        public IEnumerable<MatchedWith> BestPath { get; }
    }
}
