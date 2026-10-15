namespace SoundFingerprinting
{
    using System.Threading;
    using System.Threading.Tasks;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    /// <summary>
    ///  Query fingerprint service interface.
    /// </summary>
    public interface IQueryFingerprintService
    {
        /// <summary>
        ///   Query the underlying data source capturing best candidates and their similarity information.
        /// </summary>
        /// <param name="hashes">Hashed fingerprints generated from the query source.</param>
        /// <param name="configuration">Query configuration.</param>
        /// <param name="queryService">Storage service used to access the data source.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Query results.</returns>
        Task<QueryResult> QueryAsync(Hashes hashes, QueryConfiguration configuration, IQueryService queryService, CancellationToken cancellationToken = default);
    }
}
