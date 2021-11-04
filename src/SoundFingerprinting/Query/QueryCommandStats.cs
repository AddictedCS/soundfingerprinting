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
            return new QueryCommandStats(TotalTracksAnalyzed, TotalFingerprintsAnalyzed, QueryDurationMilliseconds, fingerprintingDurationMilliseconds);
        }

        /// <summary>
        /// Sums this instance with provided instance.
        /// </summary>
        /// <param name="stats">Status to sum with.</param>
        /// <returns>New  instance of <see cref="QueryCommandStats"/>.</returns>
        public QueryCommandStats Sum(QueryCommandStats? stats)
        {
            if (stats == null)
            {
                return this;
            }
            
            return new QueryCommandStats(TotalTracksAnalyzed + stats.TotalTracksAnalyzed, 
                TotalFingerprintsAnalyzed + stats.TotalFingerprintsAnalyzed,
                QueryDurationMilliseconds + stats.QueryDurationMilliseconds,
                FingerprintingDurationMilliseconds + stats.FingerprintingDurationMilliseconds);
        }

        /// <summary>
        ///  Returns a zeroed query stats object.
        /// </summary>
        /// <returns>An zeroed instance of the <see cref="QueryCommandStats"/> object.</returns>
        public static QueryCommandStats Zero()
        {
            return new QueryCommandStats(0, 0, 0, 0);
        }
    }
}
