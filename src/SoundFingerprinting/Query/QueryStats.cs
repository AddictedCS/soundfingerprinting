namespace SoundFingerprinting.Query
{
    /// <summary>
    ///  Query statistics
    /// </summary>
    public class QueryStats
    {
        /// <summary>
        ///  Total duration in milliseconds spent in fingerprinting and querying the data-source
        /// </summary>
        public long TotalDuration
        {
            get
            {
                return QueryDuration + FingerprintingDuration;
            }
        }

        /// <summary>
        ///  Time in milliseconds spent querying the data-source
        /// </summary>
        public long QueryDuration { get; internal set; }

        /// <summary>
        ///  Time in milliseconds spent in generating fingerprints, before querying the data-source
        /// </summary>
        public long FingerprintingDuration { get; internal set; }

        /// <summary>
        ///  Number of total tracks analyzed during querying
        /// </summary>
        public int TotalTracksAnalyzed { get; internal set; }

        /// <summary>
        ///  Number of total subfingerprints analyzed during querying. Consider fine-tuning your query/fingerprint algorithm if this number exceeds 100.
        /// </summary>
        public int TotalFingerprintsAnalyzed { get; internal set; }
    }
}
