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
        ///  Time in milliseconds spent querying the data-source
        /// </summary>
        public long QueryTime { get; internal set; }

        /// <summary>
        ///  Time in milliseconds spent in generating fingerprints, before querying the data-source
        /// </summary>
        public long FingerprintingTime { get; internal set; }

        internal static QueryResult EmptyResult()
        {
            return new QueryResult(Enumerable.Empty<ResultEntry>());
        }

        internal static QueryResult NonEmptyResult(IEnumerable<ResultEntry> results)
        {
            return new QueryResult(results);
        }
    }
}