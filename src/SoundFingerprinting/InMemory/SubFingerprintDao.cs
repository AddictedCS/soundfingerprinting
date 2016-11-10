namespace SoundFingerprinting.InMemory
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Math;

    internal class SubFingerprintDao : ISubFingerprintDao
    {
        private static long counter;

        private readonly IRAMStorage storage;
        private readonly IHashConverter hashConverter;

        public SubFingerprintDao()
            : this(DependencyResolver.Current.Get<IRAMStorage>(), DependencyResolver.Current.Get<IHashConverter>())
        {
            // no op
        }

        public SubFingerprintDao(IRAMStorage storage, IHashConverter hashConverter)
        {
            this.storage = storage;
            this.hashConverter = hashConverter;
        }

        public SubFingerprintData ReadSubFingerprint(IModelReference subFingerprintReference)
        {
            if (storage.SubFingerprints.ContainsKey(subFingerprintReference))
            {
                return storage.SubFingerprints[subFingerprintReference];
            }

            return null;
        }

        public IModelReference InsertSubFingerprint(byte[] signature, int sequenceNumber, double sequenceAt, IModelReference trackReference)
        {
            var subFingerprintReference = new ModelReference<long>(Interlocked.Increment(ref counter));
            long[] longs = hashConverter.ToLongs(signature, 25);
            storage.SubFingerprints[subFingerprintReference] = new SubFingerprintData(longs, sequenceNumber, sequenceAt, subFingerprintReference, trackReference);
            if (!storage.TracksHashes.ContainsKey(trackReference))
            {
                storage.TracksHashes[trackReference] = new ConcurrentDictionary<IModelReference, HashedFingerprint>();
            }

            storage.TracksHashes[trackReference][subFingerprintReference] = new HashedFingerprint(signature, null, sequenceNumber, sequenceAt);
            return subFingerprintReference;
        }
    }
}
