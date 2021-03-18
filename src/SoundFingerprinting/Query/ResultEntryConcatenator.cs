namespace SoundFingerprinting.Query
{
    using System;
    using System.Linq;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;

    /// <summary>
    ///  ResultEntryConcatenator stitches result entries into a consecutive result entry.
    /// </summary>
    public class ResultEntryConcatenator : IResultEntryConcatenator
    {
        private readonly QueryConfiguration config;

        /// <summary>
        ///  Creates new instance of ResultEntryConcatenator.
        /// </summary>
        /// <param name="config">
        ///  Query configuration used for result entry concatenation.
        /// </param>
        public ResultEntryConcatenator(QueryConfiguration config)
        {
            this.config = config;
        }

        /// <summary>
        ///  Stitches two consecutive result entries creating a new one with aggregated information about best path and coverage.
        /// </summary>
        /// <param name="left">First result entry.</param>
        /// <param name="right">Consecutive result entry.</param>
        /// <param name="queryOffset">
        ///   Shortens or lengthens (positive or negative offset) query length (affecting coverage info), in case if result entries come from overlaid queries. <see cref="RealtimeAudioSamplesAggregator"/>.
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

            float fingerprintLength = (float)config.FingerprintConfiguration.FingerprintLengthInSeconds;
            var lastMatch = left.Coverage.BestPath.Last();

            var nextBestPath = right
                .Coverage
                .BestPath
                .Select(_ => new MatchedWith(
                    _.QuerySequenceNumber + lastMatch.QuerySequenceNumber + 1 + (uint)((GetGapSize(left, fingerprintLength) + queryOffset) / fingerprintLength),
                    _.QueryMatchAt + lastMatch.QueryMatchAt + fingerprintLength + GetGapSize(left, fingerprintLength) + (float)queryOffset,
                    _.TrackSequenceNumber,
                    _.TrackMatchAt,
                    _.Score));
 
            var bestPath = left.Coverage.BestPath.Concat(nextBestPath).ToList();
            double queryLength = left.Coverage.QueryLength + right.Coverage.QueryLength + queryOffset;
            var coverage = bestPath.EstimateCoverage(queryLength, left.Coverage.TrackLength, fingerprintLength, config.PermittedGap);
            var track = left.Track;
            double score = left.Score + right.Score;
            return new ResultEntry(track, score, left.MatchedAt, coverage);
        }
        
        private static float GetGapSize(ResultEntry left, float fingerprintLength)
        {
            var lastMatch = left.Coverage.BestPath.Last();
            float endsAt = lastMatch.QueryMatchAt + fingerprintLength;
            return (float)(left.Coverage.QueryLength - endsAt);
        }
    }
}