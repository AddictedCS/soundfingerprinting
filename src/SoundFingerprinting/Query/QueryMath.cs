namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
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

        public List<ResultEntry> GetBestCandidates(
            List<HashedFingerprint> hashedFingerprints,
            GroupedQueryResults groupedQueryResults,
            int maxNumberOfMatchesToReturn,
            IModelService modelService,
            FingerprintConfiguration fingerprintConfiguration)
        {
            double queryLength = CalculateExactQueryLength(hashedFingerprints, fingerprintConfiguration);
            var trackIds = groupedQueryResults.GetTopTracksByHammingSimilarity(maxNumberOfMatchesToReturn).ToList();
            var tracks = modelService.ReadTracksByReferences(trackIds);
            return tracks.SelectMany(track => BuildResultEntries(fingerprintConfiguration, track, groupedQueryResults, queryLength))
                         .ToList();
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

        public double CalculateExactQueryLength(IEnumerable<HashedFingerprint> hashedFingerprints, FingerprintConfiguration fingerprintConfiguration)
        {
            double startsAt = double.MaxValue, endsAt = double.MinValue;
            foreach (var hashedFingerprint in hashedFingerprints)
            {
                startsAt = System.Math.Min(startsAt, hashedFingerprint.StartsAt);
                endsAt = System.Math.Max(endsAt, hashedFingerprint.StartsAt);
            }

            return SubFingerprintsToSeconds.AdjustLengthToSeconds(endsAt, startsAt, fingerprintConfiguration);
        }

        private IEnumerable<ResultEntry> BuildResultEntries(FingerprintConfiguration configuration, TrackData track, GroupedQueryResults groupedQueryResults, double queryLength)
        {
            var coverages = queryResultCoverageCalculator.GetCoverages(track, groupedQueryResults, configuration);
            return coverages.Select(
                coverage =>
                {
                    double confidence = confidenceCalculator.CalculateConfidence(
                        coverage.SourceMatchStartsAt,
                        coverage.SourceMatchLength,
                        queryLength,
                        coverage.OriginMatchStartsAt,
                        track.Length);

                    return new ResultEntry(
                        track,
                        coverage.SourceMatchStartsAt,
                        coverage.SourceMatchLength,
                        coverage.OriginMatchStartsAt,
                        GetTrackStartsAt(groupedQueryResults.GetBestMatchForTrack(track.TrackReference)),
                        confidence,
                        groupedQueryResults.GetHammingSimilaritySumForTrack(track.TrackReference),
                        queryLength);
                });
        }

        private double GetTrackStartsAt(MatchedWith bestMatch)
        {
            return bestMatch.QueryAt - bestMatch.ResultAt;
        }
    }
}
