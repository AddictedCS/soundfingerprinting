namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;

    public class QueryResult
    {
        public static QueryResult Empty()
        {
            return EmptyResult();
        }

        internal QueryResult(IEnumerable<ResultEntry> results)
        {
            ResultEntries = results;
        }

        /// <summary>
        ///   Gets a value indicating whether query result contains any matches
        /// </summary>
        public bool ContainsMatches
        {
            get
            {
                return ResultEntries != null && ResultEntries.Any();
            }
        }

        /// <summary>
        ///   Gets the list of potential matches, sorted from the most probable to the least probable
        /// </summary>
        public IEnumerable<ResultEntry> ResultEntries { get; private set; }

        /// <summary>
        ///  Gets best match if result entries are not empty
        /// </summary>
        public ResultEntry BestMatch
        {
            get
            {
                if (ContainsMatches)
                {
                    return ResultEntries.First();
                }

                return null;
            }
        }

        /// <summary>
        ///  Query statistics
        /// </summary>
        public QueryStats Stats { get; internal set; } = new QueryStats();

        internal static QueryResult EmptyResult()
        {
            return new QueryResult(Enumerable.Empty<ResultEntry>());
        }

        internal static QueryResult NonEmptyResult(IEnumerable<ResultEntry> results, int totalTracksCandidates, int totalSubFingerprintCandidates)
        {
            var queryResults = new QueryResult(results)
                               {
                                   Stats =
                                   {
                                       TotalTracksAnalyzed = totalTracksCandidates,
                                       TotalSubFingerprintsAnalyzed = totalSubFingerprintCandidates
                                   }
                               };
            return queryResults;
        }

        /// <summary>
        ///  Query statistics
        /// </summary>
        public class QueryStats
        {
            /// <summary>
            ///  Time in milliseconds spent querying the data-source
            /// </summary>
            public long QueryTime { get; internal set; }

            /// <summary>
            ///  Time in milliseconds spent in generating fingerprints, before querying the data-source
            /// </summary>
            public long FingerprintingTime { get; internal set; }

            /// <summary>
            ///  Number of total tracks analyzed during querying
            /// </summary>
            public int TotalTracksAnalyzed { get; internal set; }

            /// <summary>
            ///  Number of total subfingerprints analyzed during querying. Consider fine-tuning your query/fingerprint algorithm if this number exceeds 100.
            /// </summary>
            public int TotalSubFingerprintsAnalyzed { get; internal set; }
        }
    }
}
