namespace SoundFingerprinting.Dao.RAM
{
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
                SubFingerprintData subFingerprint = new SubFingerprintData(
                    signature, new ModelReference<long>(counter), new ModelReference<int>(trackId));
                storage.SubFingerprints[counter] = subFingerprint;
                return counter;
            }
        }
    }
}
