namespace SoundFingerprinting.Query
{
    internal interface IRealtimeAggregator
    {
        /// <summary>
        ///  Consume candidates returned from the <see cref="QueryFingerprintService.Query"/> invocation.
        /// </summary>
        /// <param name="candidates">And instance of <see cref="QueryResult"/> class.</param>
        /// <returns>
        ///  Instance of <see cref="RealtimeQueryResult"/>.
        /// </returns>
        RealtimeQueryResult Consume(AVQueryResult? candidates);

        /// <summary>
        ///  Purge any pending results if any.
        /// </summary>
        /// <returns>Instance of <see cref="RealtimeQueryResult"/>.</returns>
        RealtimeQueryResult Purge();
    }
}