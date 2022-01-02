namespace SoundFingerprinting.Query
{
    using System;
    using System.Linq;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.LCS;

    /// <summary>
    ///  ResultEntryConcatenator stitches result entries into a consecutive result entry.
    /// </summary>
    public class ResultEntryConcatenator : IConcatenator<ResultEntry>
    {
        /// <summary>
        ///  Stitches two consecutive result entries creating a new one with aggregated information about best path and coverage.
        /// </summary>
        /// <param name="left">First result entry.</param>
        /// <param name="right">Consecutive result entry.</param>
        /// <param name="queryOffset">
        ///   Lengthens or shortens (positive or negative offset) query length (affecting coverage info, <see cref="MatchedWith.QueryMatchAt"/> and <see cref="MatchedWith.QuerySequenceNumber"/>),
        ///   in case if result entries come from overlapped queries. <see cref="RealtimeAudioSamplesAggregator.Aggregate"/>.
        /// </param>
        /// <returns>Concatenated result entry</returns>
        /// <exception cref="ArgumentException">
        ///  ResultEntry concatenation only makes sense when applied on the same track.
        /// </exception>
        public ResultEntry Concat(ResultEntry? left, ResultEntry? right, double queryOffset = 0)
        {
            if (left == null || right == null)
            {
                return (left ?? right)!;
            }

            if (!left.Track.Id.Equals(right.Track.Id, StringComparison.InvariantCulture))
            {
                throw new ArgumentException($"{nameof(left.Track.Id)} mismatch: '{left.Track.Id}' != '{right.Track.Id}'");
            }
            
            if (left.Coverage.Contains(right.Coverage))
            {
                return left;
            }

            if (right.Coverage.Contains(left.Coverage))
            {
                return right;
            }

            if (left.TrackMatchStartsAt > right.TrackMatchStartsAt)
            {
                (left, right) = (right, left);
            }

            float fingerprintLength = (float)left.Coverage.FingerprintLength;
            var leftLastMatch = left.Coverage.BestPath.Last();
            float gapSize = GetGapSize(leftLastMatch, left.QueryLength, fingerprintLength);
            
            var nextBestPath = right
                .Coverage
                .BestPath
                .Select(_ => new MatchedWith(
                    querySequenceNumber: _.QuerySequenceNumber + leftLastMatch.QuerySequenceNumber + 1 + (uint)((gapSize + queryOffset) / fingerprintLength),
                    queryMatchAt: _.QueryMatchAt + leftLastMatch.QueryMatchAt + fingerprintLength + gapSize + (float)queryOffset,
                    trackSequenceNumber: _.TrackSequenceNumber,
                    trackMatchAt: _.TrackMatchAt,
                    score: _.Score));
 
            var bestPath = left.Coverage.BestPath.Concat(nextBestPath).ToList();
            double queryLength = left.Coverage.QueryLength + right.Coverage.QueryLength + queryOffset;
            var coverage = bestPath.EstimateCoverage(queryLength, left.Coverage.TrackLength, fingerprintLength, left.Coverage.PermittedGap);
            var track = left.Track;
            double score = left.Score + right.Score;
            return new ResultEntry(track, score, left.MatchedAt, coverage);
        }

        public ResultEntry WithExtendedQueryLength(ResultEntry old, double length)
        {
            return new ResultEntry(old.Track, old.Score, old.MatchedAt, new Coverage(old.Coverage.BestPath, old.Coverage.QueryLength + length, old.Coverage.TrackLength, old.Coverage.FingerprintLength, old.Coverage.PermittedGap));
        }

        private static float GetGapSize(MatchedWith leftLastMatch, double leftQueryLength, float fingerprintLength)
        {
            float endsAt = leftLastMatch.QueryMatchAt + fingerprintLength;
            return (float)leftQueryLength - endsAt;
        }
    }
}