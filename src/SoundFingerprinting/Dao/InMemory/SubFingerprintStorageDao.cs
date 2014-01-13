namespace SoundFingerprinting.Dao.InMemory
{
    using SoundFingerprinting.Dao.Internal;
    using SoundFingerprinting.Data;

    internal class SubFingerprintStorageDao : ISubFingerprintDao
    {
        private static long counter;

        private readonly object lockObject = new object();

        private readonly RAMStorage storage;

        public SubFingerprintStorageDao()
        {
            storage = RAMStorage.Instance;
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
                SubFingerprintData subFingerprint = new SubFingerprintData(
                    signature, new ModelReference<long>(counter), new ModelReference<int>(trackId));
                storage.SubFingerprints[counter] = subFingerprint;
                return counter;
            }
        }
    }
}
