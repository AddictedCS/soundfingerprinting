namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    public interface IQueryFingerprintService
    {
        QueryResult Query(
            IModelService modelService,
            IEnumerable<HashedFingerprint> hashedFingerprints,
            QueryConfiguration queryConfiguration);

        QueryResult Query2(
            IModelService modelService,
            IEnumerable<HashedFingerprint> hashedFingerprints,
            QueryConfiguration queryConfiguration);

        QueryResult Query3(
          IModelService modelService,
          IEnumerable<HashedFingerprint> hashedFingerprints,
          QueryConfiguration queryConfiguration);
    }
}