namespace SoundFingerprinting.Query
{
    /// <summary>
    ///  Query statistics class.
    /// </summary>
    public class QueryCommandStats
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="QueryCommandStats"/> class.
        /// </summary>
        /// <param name="totalTracksAnalyzed">Total number of analyzed tracks.</param>
        /// <param name="totalFingerprintsAnalyzed">Total number of fingerprints analyzed.</param>
        /// <param name="queryDurationMilliseconds">Query duration measured in milliseconds.</param>
        /// <param name="fingerprintingDurationMilliseconds">Fingerprint duration measured in milliseconds.</param>
        public QueryCommandStats(int totalTracksAnalyzed, int totalFingerprintsAnalyzed, long queryDurationMilliseconds, long fingerprintingDurationMilliseconds)
        {
            TotalTracksAnalyzed = totalTracksAnalyzed;
            TotalFingerprintsAnalyzed = totalFingerprintsAnalyzed;
            QueryDurationMilliseconds = queryDurationMilliseconds;
            FingerprintingDurationMilliseconds = fingerprintingDurationMilliseconds;
        }
        
        /// <summary>
        ///  Gets total duration in milliseconds spent in fingerprinting and querying the data-source.
        /// </summary>
        public long TotalDurationMilliseconds => QueryDurationMilliseconds + FingerprintingDurationMilliseconds;

        /// <summary>
        ///  Gets duration in milliseconds spent querying the data-source.
        /// </summary>
        public long QueryDurationMilliseconds { get; }

        /// <summary>
        ///  Gets duration in milliseconds spent in generating fingerprints, before querying the data-source.
        /// </summary>
        public long FingerprintingDurationMilliseconds { get; }

        /// <summary>
        ///  Gets number of total tracks analyzed during querying.
        /// </summary>
        public int TotalTracksAnalyzed { get; }

        /// <summary>
        ///  Gets number of total fingerprints analyzed during querying. Consider fine-tuning your query/fingerprint algorithm if this number exceeds 100.
        /// </summary>
        public int TotalFingerprintsAnalyzed { get; }

        /// <summary>
        ///  Creates a copy of current query stats object with updated fingerprinting duration.
        /// </summary>
        /// <param name="fingerprintingDurationMilliseconds">Fingerprinting duration in milliseconds.</param>
        /// <returns>New instance of <see cref="QueryCommandStats"/> class.</returns>
        public QueryCommandStats WithFingerprintingDurationMilliseconds(long fingerprintingDurationMilliseconds)
        {
            return new QueryCommandStats(TotalTracksAnalyzed, TotalFingerprintsAnalyzed, QueryDurationMilliseconds, FingerprintingDurationMilliseconds);
        }
    }
}
