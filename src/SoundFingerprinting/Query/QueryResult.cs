namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;

    public class QueryResult
    {
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
        ///   Gets the list of matches, sorted from the most probable to the least probable
        /// </summary>
        public IEnumerable<ResultEntry> ResultEntries { get; }

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
        ///  Gets query statistics
        /// </summary>
        public QueryStats Stats { get; internal set; } = new QueryStats();

        public static QueryResult Empty()
        {
            return EmptyResult();
        }

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
                                       TotalFingerprintsAnalyzed = totalSubFingerprintCandidates
                                   }
                               };
            return queryResults;
        }
    }
}
