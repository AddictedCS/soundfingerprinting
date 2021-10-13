namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;

    internal interface IRealtimeResultEntryAggregator
    {
        /// <summary>
        ///  Consume candidates returned from the <see cref="QueryFingerprintService.Query"/> invocation.
        /// </summary>
        /// <param name="candidates">List of candidates.</param>
        /// <param name="queryLength">Query length (in seconds).</param>
        /// <param name="queryOffset">
        ///   Query time offset (in seconds). For more details <see cref="ResultEntryConcatenator.Concat"/>.
        /// </param>
        /// <returns>
        ///  Instance of <see cref="RealtimeQueryResult"/>.
        /// </returns>
        RealtimeQueryResult Consume(IEnumerable<ResultEntry>? candidates, double queryLength, double queryOffset);

        /// <summary>
        ///  Purge any pending results if any.
        /// </summary>
        /// <returns>Instance of <see cref="RealtimeQueryResult"/>.</returns>
        RealtimeQueryResult Purge();
    }
}