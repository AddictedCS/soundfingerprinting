namespace SoundFingerprinting.DAO
{
    using System.Linq;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public class UIntModelReferenceTracker : IModelReferenceTracker<uint>
    {
        private readonly long maxAllowedReference;
        private UIntModelReferenceProvider trackReferenceProvider;
        private UIntModelReferenceProvider subFingerprintsReferenceProvider;

        public UIntModelReferenceTracker(uint trackRef = 0, uint subFingerprintRef = 0, long maxAllowedReference = int.MaxValue)
        {
            this.maxAllowedReference = maxAllowedReference;
            trackReferenceProvider = new UIntModelReferenceProvider(trackRef, maxAllowedReference);
            subFingerprintsReferenceProvider = new UIntModelReferenceProvider(subFingerprintRef, maxAllowedReference);
        }
        
        public bool TryResetTrackRef(uint maxTrackRef)
        {
            if (maxTrackRef > trackReferenceProvider.Current)
            {
                trackReferenceProvider = new UIntModelReferenceProvider(maxTrackRef, maxAllowedReference);
                return true;
            }

            return false;
        }

        public bool TryResetSubFingerprintRef(uint maxSubFingerprintRef)
        {
            if (maxSubFingerprintRef > subFingerprintsReferenceProvider.Current)
            {
                subFingerprintsReferenceProvider = new UIntModelReferenceProvider(maxSubFingerprintRef, maxAllowedReference);
                return true;
            }

            return false;
        }

        public LinkedDataModels AssignModelReferences(TrackInfo trackInfo, Hashes hashes)
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