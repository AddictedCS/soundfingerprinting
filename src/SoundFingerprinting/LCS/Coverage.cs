namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;

    using SoundFingerprinting.Query;

    public class Coverage
    {
        public Coverage(double sourceMatchStartsAt, double sourceMatchLength, double sourceCoverageLength, double originMatchStartsAt, double trackStartsAt, double queryLength, double scoreSumAcrossCoverage, double scoreAvgAcrossCoverage, int queryMatchesCountAcrossCoverage, int sourceMatchesCountAcrossCoverage, IEnumerable<MatchedWith> bestPath)
        {
            SourceMatchStartsAt = sourceMatchStartsAt;
            SourceMatchLength = sourceMatchLength;
            OriginMatchStartsAt = originMatchStartsAt;
            TrackStartsAt = trackStartsAt;
            SourceCoverageLength = sourceCoverageLength;
            QueryLength = queryLength;
            ScoreSumAcrossCoverage = scoreSumAcrossCoverage;
            ScoreAvgAcrossCoverage = scoreAvgAcrossCoverage;
            QueryMatchesCountAcrossCoverage = queryMatchesCountAcrossCoverage;
            SourceMatchesCountAcrossCoverage = sourceMatchesCountAcrossCoverage;
            BestPath = bestPath;
        }

        public double SourceMatchStartsAt { get; }

        public double SourceMatchLength { get; }

        public double SourceCoverageLength { get; }

        public double OriginMatchStartsAt { get; }

        public double TrackStartsAt { get; }

        public double QueryLength { get; }

        public double ScoreSumAcrossCoverage { get; }

        public double ScoreAvgAcrossCoverage { get; }

        public int QueryMatchesCountAcrossCoverage { get; }

        public int SourceMatchesCountAcrossCoverage { get; }

        public IEnumerable<MatchedWith> BestPath { get; }
    }
}
