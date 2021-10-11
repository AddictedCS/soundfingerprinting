namespace SoundFingerprinting.Query
{
    /// <summary>
    ///  Query statistics class.
    /// </summary>
    public class QueryStats
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="QueryStats"/> class.
        /// </summary>
        /// <param name="totalTracksAnalyzed">Total number of analyzed tracks.</param>
        /// <param name="totalFingerprintsAnalyzed">Total number of fingerprints analyzed.</param>
        /// <param name="queryDuration">Query duration measured in milliseconds.</param>
        /// <param name="fingerprintingDuration">Fingerprint duration measured in milliseconds.</param>
        public QueryStats(int totalTracksAnalyzed, int totalFingerprintsAnalyzed, long queryDuration, long fingerprintingDuration)
        {
            TotalTracksAnalyzed = totalTracksAnalyzed;
            TotalFingerprintsAnalyzed = totalFingerprintsAnalyzed;
            QueryDuration = queryDuration;
            FingerprintingDuration = fingerprintingDuration;
        }
        
        /// <summary>
        ///  Gets total duration in milliseconds spent in fingerprinting and querying the data-source.
        /// </summary>
        public long TotalDuration => QueryDuration + FingerprintingDuration;

        /// <summary>
        ///  Gets duration in milliseconds spent querying the data-source.
        /// </summary>
        public long QueryDuration { get; }

        /// <summary>
        ///  Gets duration in milliseconds spent in generating fingerprints, before querying the data-source.
        /// </summary>
        public long FingerprintingDuration { get; }

        /// <summary>
        ///  Gets number of total tracks analyzed during querying.
        /// </summary>
        public int TotalTracksAnalyzed { get; }

        /// <summary>
        ///  Gets number of total fingerprints analyzed during querying. Consider fine-tuning your query/fingerprint algorithm if this number exceeds 100.
        /// </summary>
        public int TotalFingerprintsAnalyzed { get; }
    }
}
