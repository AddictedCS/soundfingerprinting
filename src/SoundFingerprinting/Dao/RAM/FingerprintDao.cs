namespace SoundFingerprinting.Dao.RAM
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    internal class FingerprintDao : IFingerprintDao
    {
        private static int counter;

        private readonly IRAMStorage storage;

        public FingerprintDao()
            : this(DependencyResolver.Current.Get<IRAMStorage>())
        {
            // no op   
        }

        public FingerprintDao(IRAMStorage storage)
        {
            this.storage = storage;
        }

        public IModelReference InsertFingerprint(bool[] signature, IModelReference trackReference)
        {
            if (!storage.Fingerprints.ContainsKey(trackReference))
            {
                storage.Fingerprints[trackReference] = new List<FingerprintData>();
            }

            var fingerprintReference = new ModelReference<int>(Interlocked.Increment(ref counter));

            storage.Fingerprints[trackReference].Add(
                new FingerprintData(signature, trackReference) { FingerprintReference = fingerprintReference });
            return fingerprintReference;
        }

        public IList<FingerprintData> ReadFingerprintsByTrackReference(IModelReference trackReference)
        {
            if (storage.Fingerprints.ContainsKey(trackReference))
            {
                return storage.Fingerprints[trackReference];
            }

            return Enumerable.Empty<FingerprintData>().ToList();
        }
    }
}
