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
                return [];
            }
            
            double queryLength = groupedQueryResults.QueryLength;
            return matches.GetCoverages(configuration.QueryPathReconstructionStrategy, queryLength, trackData.Length, fingerprintConfiguration.FingerprintLengthInSeconds, configuration.PermittedGap);
        }
    }
}
