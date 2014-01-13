namespace SoundFingerprinting.Dao.RAM
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    internal class FingerprintDao : IFingerprintDao
    {
        private static int counter;

        private readonly object lockObject = new object();

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

        public int Insert(bool[] signature, int trackId)
        {
            lock (lockObject)
            {
                counter++;

                if (!storage.Fingerprints.ContainsKey(trackId))
                {
                    storage.Fingerprints[trackId] = new List<FingerprintData>();
                }

                storage.Fingerprints[trackId].Add(
                    new FingerprintData(signature, new ModelReference<int>(trackId))
                        {
                            FingerprintReference = new ModelReference<int>(counter) 
                        });
                return counter;
            }
        }

        public IList<FingerprintData> ReadFingerprintsByTrackId(int trackId)
        {
            if (storage.Fingerprints.ContainsKey(trackId))
            {
                return storage.Fingerprints[trackId];
            }

            return Enumerable.Empty<FingerprintData>().ToList();
        }
    }
}
