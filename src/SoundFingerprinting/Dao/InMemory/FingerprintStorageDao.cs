namespace SoundFingerprinting.Dao.InMemory
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Data;

    public class FingerprintStorageDao
    {
        private static int counter;

        private readonly object lockObject = new object();

        private readonly RAMStorage storage;

        public FingerprintStorageDao()
        {
            storage = RAMStorage.Instance;
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
