namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.SFM;

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
            var trackProfile = DecodeTrackProfile(trackData, configuration);
            return matches.GetCoverages(
                configuration.QueryPathReconstructionStrategy,
                queryLength,
                trackData.Length,
                fingerprintConfiguration.FingerprintLengthInSeconds,
                configuration.PermittedGap,
                configuration.SfmMatchStrategy,
                groupedQueryResults.QueryProfile,
                trackProfile);
        }

        private static SpectralProfile? DecodeTrackProfile(TrackData trackData, QueryConfiguration configuration)
        {
            if (configuration.SfmMatchStrategy is NoBridgingStrategy)
            {
                return null;
            }

            if (trackData.MetaFields == null || !trackData.MetaFields.TryGetValue(SpectralProfileKeys.SpectralProfile, out var base64))
            {
                return null;
            }

            return SpectralProfileCodecRegistry.Default.Decode(base64);
        }
    }
}
