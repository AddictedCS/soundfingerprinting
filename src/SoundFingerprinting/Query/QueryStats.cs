namespace SoundFingerprinting.Query
{
    /// <summary>
    ///  Query statistics
    /// </summary>
    public class QueryStats
    {
        public QueryStats(int totalTracksAnalyzed, int totalFingerprintsAnalyzed, long queryDuration, long fingerprintingDuration)
        {
            TotalTracksAnalyzed = totalTracksAnalyzed;
            TotalFingerprintsAnalyzed = totalFingerprintsAnalyzed;
            QueryDuration = queryDuration;
            FingerprintingDuration = fingerprintingDuration;
        }
        
        /// <summary>
        ///  Total duration in milliseconds spent in fingerprinting and querying the data-source
        /// </summary>
        public long TotalDuration => QueryDuration + FingerprintingDuration;

        /// <summary>
        ///  Time in milliseconds spent querying the data-source
        /// </summary>
        public long QueryDuration { get; }

        /// <summary>
        ///  Time in milliseconds spent in generating fingerprints, before querying the data-source
        /// </summary>
        public long FingerprintingDuration { get; }

        /// <summary>
        ///  Number of total tracks analyzed during querying
        /// </summary>
        public int TotalTracksAnalyzed { get; }

        /// <summary>
        ///  Number of total subfingerprints analyzed during querying. Consider fine-tuning your query/fingerprint algorithm if this number exceeds 100.
        /// </summary>
        public int TotalFingerprintsAnalyzed { get; }
    }
}
