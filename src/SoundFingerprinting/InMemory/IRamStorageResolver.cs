namespace SoundFingerprinting.InMemory
{
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Math;

    public class RamStorageResolver : ISubFingerprintIdsToDataResolver
    {
        private readonly IRAMStorage storage;

        public RamStorageResolver(IRAMStorage storage)
        {
            this.storage = storage;
        }
        
        public IEnumerable<SubFingerprintData> ResolveFromIds(IEnumerable<uint> ids, ISet<string> clusters)
        {
            if (clusters.Any())
            {
                return ids.Select(storage.ReadSubFingerprintById)
                          .Where(data => data.Clusters.Any(clusters.Contains));
            }

            return ids.Select(storage.ReadSubFingerprintById);
        }
    }
}