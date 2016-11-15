namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    public interface IQueryFingerprintService
    {
        /// <summary>
        /// Query the underlying data source capturing best candidate and similarity information
        /// </summary>
        /// <param name="modelService">Model Service used to access the data source</param>
        /// <param name="hashedFingerprints">Hashed fingerprints from query snippet</param>
        /// <param name="queryConfiguration">Query configuration</param>
        /// <returns>Result with details</returns>
        QueryResult Query(
            IModelService modelService,
            List<HashedFingerprint> hashedFingerprints,
            QueryConfiguration queryConfiguration);

        QueryResult QueryExperimental(
            IModelService modelService,
            List<HashedFingerprint> hashedFingerprints,
            QueryConfiguration queryConfiguration);
    }
}