namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LCS;

    internal class QueryMath : IQueryMath
    {
        private readonly IQueryResultCoverageCalculator queryResultCoverageCalculator;
        private readonly IConfidenceCalculator confidenceCalculator;

        internal QueryMath(IQueryResultCoverageCalculator queryResultCoverageCalculator, IConfidenceCalculator confidenceCalculator)
        {
            this.queryResultCoverageCalculator = queryResultCoverageCalculator;
            this.confidenceCalculator = confidenceCalculator;
        }

        public List<ResultEntry> GetBestCandidates(GroupedQueryResults groupedQueryResults, int maxNumberOfMatchesToReturn, IModelService modelService, QueryConfiguration queryConfiguration)
        {
            var trackIds = groupedQueryResults.GetTopTracksByHammingSimilarity(maxNumberOfMatchesToReturn).ToList();
            var tracks = modelService.ReadTracksByReferences(trackIds);
            return tracks.SelectMany(track => BuildResultEntries(track, groupedQueryResults, queryConfiguration)).ToList();
        }

        public bool IsCandidatePassingThresholdVotes(HashedFingerprint queryFingerprint, SubFingerprintData candidate, int thresholdVotes)
        {
            int[] query = queryFingerprint.HashBins;
            int[] result = candidate.Hashes;
            int count = 0;
            for (int i = 0; i < query.Length; ++i)
            {
                if (query[i] == result[i])
                {
                    count++;
                }

                if (count >= thresholdVotes)
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<ResultEntry> BuildResultEntries(TrackData track, GroupedQueryResults groupedQueryResults, QueryConfiguration configuration)
        {
            var coverages = queryResultCoverageCalculator.GetCoverages(track, groupedQueryResults, configuration);
            return coverages.Select(coverage =>
               {
                    double confidence = confidenceCalculator.CalculateConfidence(
                        coverage.SourceMatchStartsAt,
                        coverage.SourceMatchLength,
                        coverage.QueryLength,
                        coverage.OriginMatchStartsAt,
                        track.Length);

                    return new ResultEntry(
                        track,
                        coverage.SourceMatchStartsAt,
                        coverage.SourceMatchLength,
                        coverage.SourceCoverageLength,
                        coverage.OriginMatchStartsAt,
                        coverage.TrackStartsAt,
                        confidence,
                        groupedQueryResults.GetHammingSimilaritySumForTrack(track.TrackReference),
                        coverage.QueryLength);
               });
        }
    }
}
