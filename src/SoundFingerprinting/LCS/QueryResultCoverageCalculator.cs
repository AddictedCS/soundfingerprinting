namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Query;

    internal class QueryResultCoverageCalculator : IQueryResultCoverageCalculator
    {
        private readonly ILongestIncreasingTrackSequence longestIncreasingTrackSequence;

        public QueryResultCoverageCalculator(ILongestIncreasingTrackSequence longestIncreasingTrackSequence)
        {
            this.longestIncreasingTrackSequence = longestIncreasingTrackSequence;
        }

        public IEnumerable<Coverage> GetCoverages(TrackData trackData, GroupedQueryResults groupedQueryResults, QueryConfiguration configuration)
        {
            var fingerprintConfiguration = configuration.FingerprintConfiguration;
            var matches = groupedQueryResults.GetMatchesForTrack(trackData.TrackReference);
            double queryLength = groupedQueryResults.GetQueryLength();

            if (configuration.AllowMultipleMatchesOfTheSameTrackInQuery)
            {
                var sequences = longestIncreasingTrackSequence.FindAllIncreasingTrackSequences(matches, configuration.PermittedGap);
                var filtered = OverlappingRegionFilter.MergeOverlappingSequences(sequences, configuration.PermittedGap);
                return filtered.Select(matchedSequence =>
                {
                    var lengthInSeconds = fingerprintConfiguration.FingerprintLengthInSeconds;   
                    return CoverageEstimator.EstimateTrackCoverage(matchedSequence, queryLength, lengthInSeconds);
                });
            }
            
            return new List<Coverage>
                   {
                       CoverageEstimator.EstimateTrackCoverage(matches, queryLength, fingerprintConfiguration.FingerprintLengthInSeconds)
                   };
        }
    }
}
