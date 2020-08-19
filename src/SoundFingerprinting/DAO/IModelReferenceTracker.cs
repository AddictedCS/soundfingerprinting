namespace SoundFingerprinting.DAO
{
    using SoundFingerprinting.Data;

    public interface IModelReferenceTracker<in T>
    {
        bool TryResetTrackRef(T maxTrackRef);
        
        bool TryResetSubFingerprintRef(T maxSubFingerprintRef);

        LinkedDataModels AssignReferences(TrackInfo trackInfo, Hashes hashes);
    }
}