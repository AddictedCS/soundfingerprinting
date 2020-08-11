﻿// ReSharper disable UnusedMember.Local
namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;
    using ProtoBuf;
    using SoundFingerprinting.Query;

    [ProtoContract]
    public class Coverage
    {
        public Coverage(IEnumerable<MatchedWith> bestPath, double queryLength, double trackLength, double fingerprintLength, double permittedGap)
        {
            BestPath = bestPath.ToList();
            QueryLength = queryLength;
            TrackLength = trackLength;
            FingerprintLength = fingerprintLength;
            PermittedGap = permittedGap;
        }

        private Coverage()
        {
            // left for proto-buf
        }

        /// <summary>
        ///  Gets starting point of the query in seconds
        /// </summary>
        public double QueryMatchStartsAt => BestPath.First().QueryMatchAt;

        /// <summary>
        ///  Gets starting point of the track match in seconds
        /// </summary>
        public double TrackMatchStartsAt => BestPath.First().TrackMatchAt;

        /// <summary>
        ///  Gets the total track length that was covered by the query. Exact length of matched fingerprints, not necessary consecutive.
        /// </summary>
        public double CoverageLength => DiscreteCoverageLength - BestPath.FindTrackGaps(TrackLength, 0, FingerprintLength).Where(d => !d.IsOnEdge).Sum(d => d.LengthInSeconds);

        /// <summary>
        ///  Gets coverage length sum in seconds, allowing gaps specified by permitted gap query parameter
        /// </summary>
        public double CoverageWithPermittedGapsLength
        {
            get
            {
                return DiscreteCoverageLength - TrackGaps.Where(g => !g.IsOnEdge).Sum(d => d.LengthInSeconds);
            }
        }
        
        /// <summary>
        ///  Gets query coverage length sum in seconds, allowing gaps specified by permitted gap query parameter
        /// </summary>
        public double QueryCoverageWithPermittedGapsLength
        {
            get
            {
                return QueryDiscreteCoverageLength - QueryGaps.Where(g => !g.IsOnEdge).Sum(d => d.LengthInSeconds);
            }
        }

        /// <summary>
        ///  Gets the track match length including all track gaps (if any).
        /// </summary>
        public double DiscreteCoverageLength => SubFingerprintsToSeconds.MatchLengthToSeconds(BestPath.Last().TrackMatchAt, TrackMatchStartsAt, FingerprintLength);

        /// <summary>
        ///  Gets the query match length including all query gaps (if any).
        /// </summary>
        public double QueryDiscreteCoverageLength => SubFingerprintsToSeconds.MatchLengthToSeconds(BestPath.Last().QueryMatchAt, QueryMatchStartsAt, FingerprintLength);

        /// <summary>
        ///  Gets the exact length of not covered portion of the query match in the database track
        /// </summary>
        /// <returns>Seconds of not covered length</returns>
        public double GapsCoverageLength
        {
            get
            {
                return BestPath
                    .FindTrackGaps(TrackLength, 0, FingerprintLength)
                    .Sum(gap => gap.LengthInSeconds);
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
        [ProtoMember(1)]
        public double QueryLength { get; }
        
        /// <summary>
        ///  Gets track length
        /// </summary>
        [ProtoMember(2)]
        public double TrackLength { get; }

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
                return BestPath.Select(m => m.QuerySequenceNumber).Distinct().Count();
            }
        }

        /// <summary>
        ///  Gets number of database fingerprints that matched the query fingerprints
        /// </summary>
        public int TrackMatchesCount
        {
            get
            {
                return BestPath.Select(m => m.TrackSequenceNumber).Distinct().Count();
            }
        }

        /// <summary>
        ///  Gets best reconstructed path
        /// </summary>
        [ProtoMember(3)]
        public IEnumerable<MatchedWith> BestPath { get; }

        /// <summary>
        ///  Gets query match gaps from the best path
        /// </summary>
        public IEnumerable<Gap> QueryGaps => BestPath.FindQueryGaps(PermittedGap, FingerprintLength);

        /// <summary>
        ///  Gets track match gaps from the best path
        /// </summary>
        public IEnumerable<Gap> TrackGaps => BestPath.FindTrackGaps(TrackLength, PermittedGap, FingerprintLength);

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

        public Coverage NewBestPath(IEnumerable<MatchedWith> newBestPath)
        {
            return new Coverage(newBestPath, QueryLength, TrackLength, FingerprintLength, PermittedGap);
        }
        
        public bool Contains(Coverage other)
        {
            return (TrackMatchStartsAt <= other.TrackMatchStartsAt && TrackMatchStartsAt + CoverageWithPermittedGapsLength >= other.TrackMatchStartsAt + other.CoverageWithPermittedGapsLength)
                   &&
                   (QueryMatchStartsAt <= other.QueryMatchStartsAt && QueryMatchStartsAt + QueryCoverageWithPermittedGapsLength >= other.QueryMatchStartsAt + other.QueryCoverageWithPermittedGapsLength);
        }

        [ProtoMember(4)]
        internal double FingerprintLength { get; }

        [ProtoMember(5)]
        internal double PermittedGap { get; }
    }
}
