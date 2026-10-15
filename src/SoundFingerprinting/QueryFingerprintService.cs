namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.SFM;

    /// <summary>
    ///  Query fingerprint service.
    /// </summary>
    /// <remarks>
    /// Please use <see cref="QueryCommandBuilder"/> for creating <see cref="QueryCommand"/> objects that will issue query to the provided <see cref="IModelService"/>.
    /// </remarks>
    public class QueryFingerprintService : IQueryFingerprintService
    {
        private readonly IQueryMath queryMath;

        private QueryFingerprintService(IQueryMath queryMath)
        {
            this.queryMath = queryMath;
        }

        /// <summary>
        ///  Gets an instance of the <see cref="QueryFingerprintService"/> class.
        /// </summary>
        public static QueryFingerprintService Instance { get; } = new (QueryMath.Instance);

        /// <inheritdoc cref="IQueryFingerprintService.QueryAsync"/>
        public async Task<QueryResult> QueryAsync(Hashes hashes, QueryConfiguration configuration, IQueryService queryService, CancellationToken cancellationToken = default)
        {
            var queryStopwatch = Stopwatch.StartNew();
            var candidates = await queryService.QueryEfficientlyAsync(hashes, configuration, cancellationToken);
            var groupedQueryResults = GroupCandidates(hashes, configuration, candidates);
            if (!groupedQueryResults.ContainsMatches)
            {
                return QueryResult.Empty(hashes,  queryStopwatch.ElapsedMilliseconds);
            }

            // the storage round trips stay here so that query math never depends on a service that can perform remote I/O
            var trackReferences = groupedQueryResults.GetTopTracksByScore(configuration.MaxTracksToReturn).ToList();
            var tracks = await queryService.ReadTracksByReferencesAsync(trackReferences, cancellationToken);
            var resultEntries = queryMath.GetBestCandidates(groupedQueryResults, tracks, configuration);
            int totalTracksAnalyzed = groupedQueryResults.TracksCount;
            int totalSubFingerprintsAnalyzed = groupedQueryResults.SubFingerprintsCount;
            return QueryResult.NonEmptyResult(resultEntries, hashes, totalTracksAnalyzed, totalSubFingerprintsAnalyzed, queryStopwatch.ElapsedMilliseconds);
        }

        private static GroupedQueryResults GroupCandidates(Hashes queryHashes, QueryConfiguration configuration, Candidates candidates)
        {
            var queryProfile = DecodeQueryProfile(queryHashes, configuration);
            var groupedResults = new GroupedQueryResults(queryHashes.DurationInSeconds, queryHashes.RelativeTo, queryProfile, queryHashes.TimeOffset);
            foreach (KeyValuePair<IModelReference, List<MatchedWith>> kv in candidates.GetMatches())
            {
                foreach (var match in kv.Value)
                {
                    groupedResults.Add(match.QuerySequenceNumber, kv.Key, match);
                }
            }

            return groupedResults;
        }

        private static SpectralProfile? DecodeQueryProfile(Hashes queryHashes, QueryConfiguration configuration)
        {
            // skip the decode when the configured strategy can't use it — saves base64 work on the hot query path
            if (configuration.SfmMatchStrategy is NoBridgingStrategy)
            {
                return null;
            }

            if (!queryHashes.Properties.TryGetValue(SpectralProfileKeys.SpectralProfile, out var base64))
            {
                return null;
            }

            return SpectralProfileCodecRegistry.Default.Decode(base64);
        }
    }
}
