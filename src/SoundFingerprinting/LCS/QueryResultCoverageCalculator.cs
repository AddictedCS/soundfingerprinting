namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Query;

    internal class QueryResultCoverageCalculator : IQueryResultCoverageCalculator
    {
        public IEnumerable<Coverage> GetCoverages(TrackData trackData, GroupedQueryResults groupedQueryResults, QueryConfiguration configuration)
        {
            var fingerprintConfiguration = configuration.FingerprintConfiguration;
            var matches = groupedQueryResults.GetMatchesForTrack(trackData.TrackReference).ToList();

            if (!matches.Any())
            {
                return Enumerable.Empty<Coverage>();
            }
            
            double queryLength = groupedQueryResults.QueryLength;

            if (configuration.AllowMultipleMatchesOfTheSameTrackInQuery)
            {
                return matches.EstimateIncreasingCoverages(queryLength, trackData.Length,
                    fingerprintConfiguration.FingerprintLengthInSeconds, configuration.PermittedGap);
            }

            return new[] { matches.EstimateCoverage(queryLength, trackData.Length, fingerprintConfiguration.FingerprintLengthInSeconds, configuration.PermittedGap) };
        }
    }
}
