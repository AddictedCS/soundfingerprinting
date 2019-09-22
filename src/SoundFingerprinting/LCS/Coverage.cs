namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Query;

    public class Coverage
    {
        private readonly double fingerprintLength;

        public Coverage(IEnumerable<MatchedWith> bestPath, double queryLength, double avgScore, int trackMatchesCount, double fingerprintLength)
        {
            BestPath = bestPath.ToList();
            QueryLength = queryLength;
            AvgScore = avgScore;
            TrackMatchesCount = trackMatchesCount;
            this.fingerprintLength = fingerprintLength;
        }

        /// <summary>
        ///  Gets starting point of the query in seconds
        /// </summary>
        public double QueryMatchStartsAt
        {
            get
            {
                return BestPath.First().QueryMatchAt;
            }
        }

        /// <summary>
        ///  Gets starting point of the track match in seconds
        /// </summary>
        public double TrackMatchStartsAt
        {
            get
            {
                return BestPath.First().TrackMatchAt;
            }
        }

        /// <summary>
        ///  Gets query coverage sum in seconds. Exact length of matched fingerprints, not necessary consecutive, just how much length has been covered by the query
        /// </summary>
        public double QueryCoverageSum
        {
            get
            {
                return MatchLengthWithTrackDiscontinuities - NotCoveredLength;
            }
        }

        /// <summary>
        ///  Gets match length including track discontinuities
        /// </summary>
        public double MatchLengthWithTrackDiscontinuities
        {
            get
            {
                return SubFingerprintsToSeconds.AdjustLengthToSeconds(BestPath.Last().TrackMatchAt, TrackMatchStartsAt, fingerprintLength);
            }
        }

        /// <summary>
        ///  Gets best estimate of where does the track actually starts.
        ///  Can be negative, if algorithm assumes the track starts in the past point relative to the query
        /// </summary>
        public double TrackStartsAt
        {
            get
            {
                var bestMatch = BestPath.OrderByDescending(m => m.Score).First();
                return bestMatch.QueryMatchAt - bestMatch.TrackMatchAt;
            }
        }

        /// <summary>
        ///  Gets query length
        /// </summary>
        public double QueryLength { get; }

        /// <summary>
        ///  Gets average score across all track matches
        /// </summary>
        public double AvgScore { get; }

        /// <summary>
        ///  Gets average score across best path
        /// </summary>
        public double AvgScoreAcrossBestPath
        {
            get
            {
                return BestPath.Average(m => m.Score);
            }
        }

        /// <summary>
        ///  Gets number of query fingerprints that matched the database track
        /// </summary>
        public int QueryMatchesCount
        {
            get
            {
                return BestPath.Count();
            }
        }

        /// <summary>
        ///  Gets number of database fingerprints that matched the query
        /// </summary>
        public int TrackMatchesCount { get; }

        /// <summary>
        ///  Gets best reconstructed path
        /// </summary>
        public IEnumerable<MatchedWith> BestPath { get; }

        /// <summary>
        ///  Computes query match discontinuities. Capture all the query gaps we find in the best path
        /// </summary>
        /// <param name="permittedGap">Permitted gap</param>
        /// <returns>Set of discontinuities</returns>
        public IEnumerable<Discontinuity> ComputeQueryDiscontinuities(double permittedGap)
        {
            return BestPath.Select(m => m.QueryMatchAt).FindGaps(permittedGap);
        }

        /// <summary>
        ///  Computes track match discontinuities. Capture all the track gaps we find in the best path
        /// </summary>
        /// <param name="permittedGap">Permitted gap</param>
        /// <returns>Set of discontinuities</returns>
        public IEnumerable<Discontinuity> ComputeTrackDiscontinuities(double permittedGap)
        {
            return BestPath.Select(m => m.TrackMatchAt).FindGaps(permittedGap);
        }

        /// <summary>
        ///  Get score outliers from the best path. Useful to find regions which are weak matches and may require additional recheck
        /// </summary>
        /// <param name="sigma">Allowed deviation from the mean</param>
        /// <returns>Set of score outliers</returns>
        public IEnumerable<MatchedWith> GetScoreOutliers(double sigma)
        {
            var list = BestPath.ToList();
            double stdDev = list.Select(m => m.Score).StdDev();
            double avg = list.Average(m => m.Score);
            return list.Where(match => match.Score < avg - sigma * stdDev);
        }

        /// <summary>
        ///  Gets the exact length of not covered portion of the query match in the database track
        /// </summary>
        /// <returns>Seconds of not covered length</returns>
        public double NotCoveredLength
        {
            get
            {
                double notCoveredSeconds = 0d;
                var list = BestPath.ToList();
                for (int i = 1; i < list.Count - 1; ++i)
                {
                    if (list[i].TrackMatchAt - list[i - 1].TrackMatchAt > fingerprintLength)
                    {
                        notCoveredSeconds += list[i].TrackMatchAt - (list[i - 1].TrackMatchAt + fingerprintLength);
                    }
                }

                return notCoveredSeconds;
            }
        }
    }
}
