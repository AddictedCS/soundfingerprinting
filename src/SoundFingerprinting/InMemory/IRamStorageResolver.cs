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
        
        public IEnumerable<SubFingerprintData> ResolveFromIds(IEnumerable<uint> ids)
        {
            return ids.Select(storage.ReadSubFingerprintById);
        }
    }
}