namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    internal interface IQueryFingerprintService
    {
        /// <summary>
        ///   Query the underlying data source capturing best candidates and their similarity information
        /// </summary>
        /// <param name="queryFingerprints">Hashed fingerprints generated from the query source</param>
        /// <param name="configuration">Query configuration</param>
        /// <param name="modelService">Storage service used to access the data source</param>
        /// <returns>Query results</returns>
        QueryResult Query(List<HashedFingerprint> queryFingerprints, QueryConfiguration configuration, IModelService modelService);
    }
}