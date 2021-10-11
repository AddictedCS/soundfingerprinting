namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Object containing information about the query result.
    /// </summary>
    public class QueryResult
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="QueryResult"/> class.
        /// </summary>
        /// <param name="results">Actual result entries</param>
        /// <param name="queryHashes">Query hashes, used for querying</param>
        /// <param name="queryStats">Query statistics</param>
        public QueryResult(IEnumerable<ResultEntry> results, Hashes queryHashes, QueryStats queryStats)
        {
            ResultEntries = results;
            QueryHashes = queryHashes;
            Stats = queryStats;
        }

        /// <summary>
        ///   Gets a value indicating whether query result contains any matches.
        /// </summary>
        public bool ContainsMatches => ResultEntries != null && ResultEntries.Any();

        /// <summary>
        ///   Gets the list of matches, sorted from the most probable to the least probable.
        /// </summary>
        public IEnumerable<ResultEntry> ResultEntries { get; }

        /// <summary>
        ///  Gets query hashes used for querying.
        /// </summary>
        public Hashes QueryHashes { get; }

        /// <summary>
        ///  Gets best match if result entries are not empty.
        /// </summary>
        public ResultEntry? BestMatch => ContainsMatches ? ResultEntries.First() : null;

        /// <summary>
        ///  Gets query statistics.
        /// </summary>
        public QueryStats Stats { get; }

        /// <summary>
        ///  Gets empty query result.
        /// </summary>
        /// <param name="hashes">Hashes used for querying.</param>
        /// <returns>An empty instance of the <see cref="QueryResult"/> class. </returns>
        public static QueryResult Empty(Hashes hashes)
        {
            return new QueryResult(Enumerable.Empty<ResultEntry>(), hashes, new QueryStats(0, 0, 0, 0));
        }

        /// <summary>
        ///  Returns an instance of QueryResult class that will contain a list of result entries.
        /// </summary>
        /// <param name="results">Result entries.</param>
        /// <param name="hashes">Query hashes used for querying.</param>
        /// <param name="totalTracksCandidates">Total track candidates analyzed during query.</param>
        /// <param name="totalSubFingerprintCandidates">Total sub-fingerprint candidates analyzed during query.</param>
        /// <returns>Instance of QueryResult class.</returns>
        public static QueryResult NonEmptyResult(IEnumerable<ResultEntry> results, Hashes hashes, int totalTracksCandidates, int totalSubFingerprintCandidates)
        {
            return new QueryResult(results, hashes, new QueryStats(totalTracksCandidates, totalSubFingerprintCandidates, 0, 0));
        }
    }
}
