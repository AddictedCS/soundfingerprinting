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
            IEnumerable<HashedFingerprint> hashedFingerprints,
            QueryConfiguration queryConfiguration);

        /// <summary>
        /// Query the underlying data source capturing best candidate as well as time location within the track
        /// </summary>
        /// <param name="modelService">Model Service used to access the data source</param>
        /// <param name="hashedFingerprints">Hashed fingerprints from query snippet</param>
        /// <param name="queryConfiguration">Query configuration</param>
        /// <returns>Result with details</returns>
        QueryResult QueryWithTimeSequenceInformation(
            IModelService modelService,
            IEnumerable<HashedFingerprint> hashedFingerprints,
            QueryConfiguration queryConfiguration);

        QueryResult QueryExperimental(IModelService modelService, IEnumerable<HashedFingerprint> hashedFingerprints, QueryConfiguration queryConfiguration);
    }
}