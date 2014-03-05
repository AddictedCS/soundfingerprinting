namespace SoundFingerprinting.InMemory
{
    using System.Collections.Concurrent;
    using System.Threading;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    internal class SubFingerprintDao : ISubFingerprintDao
    {
        private static long counter;

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

        public SubFingerprintData ReadSubFingerprint(IModelReference subFingerprintReference)
        {
            if (storage.SubFingerprints.ContainsKey(subFingerprintReference))
            {
                return storage.SubFingerprints[subFingerprintReference];
            }

            return null;
        }

        public IModelReference InsertSubFingerprint(byte[] signature, IModelReference trackReference)
        {
            var subFingerprintReference = new ModelReference<long>(Interlocked.Increment(ref counter));
            storage.SubFingerprints[subFingerprintReference] = new SubFingerprintData(signature, subFingerprintReference, trackReference);
            if (!storage.TracksHashes.ContainsKey(trackReference))
            {
                storage.TracksHashes[trackReference] = new ConcurrentDictionary<IModelReference, HashData>();
            }

            storage.TracksHashes[trackReference][subFingerprintReference] = new HashData { SubFingerprint = signature };
            return subFingerprintReference;
        }
    }
}
