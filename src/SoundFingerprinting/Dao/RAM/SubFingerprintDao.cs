namespace SoundFingerprinting.Dao.RAM
{
    using System.Collections.Concurrent;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    internal class SubFingerprintDao : ISubFingerprintDao
    {
        private static long counter;

        private readonly object lockObject = new object();

        private readonly IRAMStorage storage;

        public SubFingerprintDao()
            : this(DependencyResolver.Current.Get<IRAMStorage>())
        {
            // no op
        }

        public SubFingerprintDao(IRAMStorage storage)
        {
            this.storage = storage;
        }

        public SubFingerprintData ReadById(long id)
        {
            if (storage.SubFingerprints.ContainsKey(id))
            {
                return storage.SubFingerprints[id];
            }

            return null;
        }

        public long Insert(byte[] signature, int trackId)
        {
            lock (lockObject)
            {
                counter++;
                SubFingerprintData subFingerprint = new SubFingerprintData(signature, new ModelReference<long>(counter), new ModelReference<int>(trackId));
                storage.SubFingerprints[counter] = subFingerprint;
                if (!storage.TracksHashes.ContainsKey(trackId))
                {
                    storage.TracksHashes[trackId] = new ConcurrentDictionary<long, HashData>();
                }

                storage.TracksHashes[trackId][counter] = new HashData { SubFingerprint = signature };
                return counter;
            }
        }
    }
}
