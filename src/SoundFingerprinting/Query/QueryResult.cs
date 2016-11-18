namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;

    public class QueryResult
    {
        internal QueryResult(IEnumerable<ResultEntry> results, int analyzedSubfingerprints)
        {
            ResultEntries = results;
            AnalyzedSubFingerprintsCount = analyzedSubfingerprints;
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
        ///   Gets the number of analyzed sub-fingerprints during querying
        /// </summary>
        internal int AnalyzedSubFingerprintsCount { get; private set; }

        internal static QueryResult EmptyResult()
        {
            return new QueryResult(Enumerable.Empty<ResultEntry>(), 0);
        }

        internal static QueryResult NonEmptyResult(IEnumerable<ResultEntry> results, int analyzedSubfingerprints)
        {
            return new QueryResult(results, analyzedSubfingerprints);
        }
    }
}