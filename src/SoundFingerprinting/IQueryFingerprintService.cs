namespace SoundFingerprinting
{
    using System;
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    public interface IQueryFingerprintService
    {
        /// <summary>
        ///   Query the underlying data source capturing best candidates and their similarity information
        /// </summary>
        /// <param name="queryFingerprints">Hashed fingerprints generated from the query source</param>
        /// <param name="configuration">Query configuration</param>
        /// <param name="relativeTo">The timestamp which is considered as a reference point of the query operation. Useful when you would like to re-query the storage with previously generated fingerprints</param>
        /// <param name="modelService">Storage service used to access the data source</param>
        /// <returns>Query results</returns>
        QueryResult Query(IEnumerable<HashedFingerprint> queryFingerprints, QueryConfiguration configuration, DateTime relativeTo, IModelService modelService);
    }
}