namespace SoundFingerprinting.DAO
{
    using System.Linq;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public class UIntModelReferenceTracker : IModelReferenceTracker<uint>
    {
        private UIntModelReferenceProvider trackReferenceProvider;
        private UIntModelReferenceProvider subFingerprintsReferenceProvider;

        public UIntModelReferenceTracker(uint trackRef = 0, uint subFingerprintRef = 0)
        {
            trackReferenceProvider = new UIntModelReferenceProvider(trackRef);
            subFingerprintsReferenceProvider = new UIntModelReferenceProvider(subFingerprintRef);
        }
        
        public bool TryResetTrackRef(uint maxTrackRef)
        {
            if (maxTrackRef > trackReferenceProvider.Current)
            {
                trackReferenceProvider = new UIntModelReferenceProvider(maxTrackRef);
                return true;
            }

            return false;
        }

        public bool TryResetSubFingerprintRef(uint maxSubFingerprintRef)
        {
            if (maxSubFingerprintRef > subFingerprintsReferenceProvider.Current)
            {
                subFingerprintsReferenceProvider = new UIntModelReferenceProvider(maxSubFingerprintRef);
                return true;
            }

            return false;
        }

        public LinkedDataModels AssignReferences(TrackInfo trackInfo, Hashes hashes)
        {
            var trackRef = trackReferenceProvider.Next();
            var trackData = new TrackData(trackInfo.Id, trackInfo.Artist, trackInfo.Title, hashes.DurationInSeconds, trackRef, trackInfo.MetaFields, trackInfo.MediaType);

            var subFingerprints = hashes.Select(hash =>
            {
                var subFingerprintReference = subFingerprintsReferenceProvider.Next();
                return new SubFingerprintData(hash.HashBins, hash.SequenceNumber, hash.StartsAt, subFingerprintReference, trackRef, hash.OriginalPoint);
            })
            .ToList();

            return new LinkedDataModels(trackData, subFingerprints);
        }
    }
}