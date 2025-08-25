namespace SoundFingerprinting.DAO
{
    using System.Linq;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    /// <inheritdoc />
    public class UIntModelReferenceTracker(uint trackRef = 0, uint subFingerprintRef = 0, long maxAllowedReference = int.MaxValue) : IModelReferenceTracker<uint>
    {
        private readonly object @object = new ();
        
        private UIntModelReferenceProvider trackReferenceProvider = new (trackRef, maxAllowedReference);
        private UIntModelReferenceProvider subFingerprintsReferenceProvider = new (subFingerprintRef, maxAllowedReference);

        /// <inheritdoc />
        public bool TryResetTrackRef(uint maxTrackRef)
        {
            if (maxTrackRef > trackReferenceProvider.Current)
            {
                trackReferenceProvider = new UIntModelReferenceProvider(maxTrackRef, maxAllowedReference);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public bool TryResetSubFingerprintRef(uint maxSubFingerprintRef)
        {
            if (maxSubFingerprintRef > subFingerprintsReferenceProvider.Current)
            {
                subFingerprintsReferenceProvider = new UIntModelReferenceProvider(maxSubFingerprintRef, maxAllowedReference);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public LinkedDataModels AssignModelReferences(TrackInfo trackInfo, Hashes hashes)
        {
            lock (@object)
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
}