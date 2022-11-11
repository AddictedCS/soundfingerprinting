namespace SoundFingerprinting.Query
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.LCS;

    /// <summary>
    ///  ResultEntryConcatenator stitches result entries into a consecutive result entry.
    /// </summary>
    public class ResultEntryConcatenator : IConcatenator<ResultEntry>
    {
        private readonly bool autoSkipDetection;
        private readonly QueryPathReconstructionStrategy queryPathReconstructionStrategy;
        private readonly ILogger<ResultEntryConcatenator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultEntryConcatenator"/> class.
        /// </summary>
        /// <param name="loggerFactory">An instance of logger factory.</param>
        /// <param name="autoSkipDetection">A flag indicating whether to enable automatic skip detection.</param>
        /// <param name="queryPathReconstructionStrategy">Query path reconstruction strategy.</param>
        public ResultEntryConcatenator(ILoggerFactory loggerFactory, bool autoSkipDetection, QueryPathReconstructionStrategy queryPathReconstructionStrategy)
        {
            if (queryPathReconstructionStrategy == QueryPathReconstructionStrategy.MultipleBestPaths)
            {
                throw new ArgumentException($"Multiple best paths reconstruction strategy cannot be used inside result entry concatenator", nameof(queryPathReconstructionStrategy));
            }
            
            this.autoSkipDetection = autoSkipDetection;
            this.queryPathReconstructionStrategy = queryPathReconstructionStrategy;
            logger = loggerFactory.CreateLogger<ResultEntryConcatenator>();
        }

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
                logger.LogDebug("Left coverage contains right one {Left} > {Right}", left, right);
                return left;
            }

            if (right.Coverage.Contains(left.Coverage))
            {
                logger.LogDebug("Right coverage contains left one {Left} < {Right}", left, right);
                return right;
            }

            if (autoSkipDetection && left.TrackMatchStartsAt > right.TrackMatchStartsAt)
            {
                logger.LogDebug("Concatenator result entry reversal detected for {Left}<->{Right}", left, right);
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

            double permittedGap = left.Coverage.PermittedGap;
            double skipLength = 0d;
            double leftEndsAt = left.TrackMatchStartsAt + left.DiscreteTrackCoverageLength;
            double matchGap = right.TrackMatchStartsAt - leftEndsAt;
            if (autoSkipDetection && matchGap > 0 && right.Coverage.TrackCoverageWithPermittedGapsLength + left.Coverage.TrackCoverageWithPermittedGapsLength + matchGap > queryLength + permittedGap)
            {
                // we have covered more than the query allowed us, realtime query came with a glitch/skip
                logger.LogDebug("Result entries {Left}->{Right} covered more than it is possible by the query length {QueryLength:0.00}. Possible skip length {Glitch:0.00}", left, right, queryLength, matchGap);
                skipLength = matchGap;
            }
            
            var coverage = bestPath.GetCoverages(queryPathReconstructionStrategy, queryLength + skipLength, left.Coverage.TrackLength, fingerprintLength, left.Coverage.PermittedGap).First();
            var track = left.Track;
            double score = left.Score + right.Score;
            return new ResultEntry(track, score, left.MatchedAt, coverage.WithExtendedQueryLength(-skipLength));
        }

        /// <summary>
        ///  Extends query length of the current result entry by creating a new instance of the <see cref="ResultEntry"/> class.
        /// </summary>
        /// <param name="old">An instance of the <see cref="ResultEntry"/> class to extend it's length.</param>
        /// <param name="length">New query length will be equal to previous result entry query length plus length.</param>
        /// <returns>A new instance of the <see cref="ResultEntry"/> class with modified query length.</returns>
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