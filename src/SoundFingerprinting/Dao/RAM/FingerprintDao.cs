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

        public IModelReference InsertFingerprint(FingerprintData fingerprint)
        {
            if (!storage.Fingerprints.ContainsKey(fingerprint.TrackReference))
            {
                storage.Fingerprints[fingerprint.TrackReference] = new List<FingerprintData>();
            }

            storage.Fingerprints[fingerprint.TrackReference].Add(fingerprint);

            return fingerprint.FingerprintReference = new ModelReference<int>(Interlocked.Increment(ref counter));
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
