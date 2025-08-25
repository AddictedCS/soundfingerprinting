namespace SoundFingerprinting.DAO
{
    using SoundFingerprinting.Data;

    public interface IModelReferenceTracker<in T>
    {
        /// <summary>
        ///  Reset track reference to <paramref name="maxTrackRef"/> if it's greater than the current one.
        /// </summary>
        /// <param name="maxTrackRef">An instance of <see cref="T"/>.</param>
        /// <returns>True if successful, otherwise false.</returns>
        bool TryResetTrackRef(T maxTrackRef);
        
        /// <summary>
        ///  Reset subfingerprint reference to <paramref name="maxSubFingerprintRef"/> if it's greater than the current one.
        /// </summary>
        /// <param name="maxSubFingerprintRef">An instance of <see cref="T"/>.</param>
        /// <returns>True if successful, otherwise false.</returns>
        bool TryResetSubFingerprintRef(T maxSubFingerprintRef);

        /// <summary>
        ///  Assign model references to track and subfingerprints.
        /// </summary>
        /// <param name="trackInfo">An instance of <see cref="TrackInfo"/>.</param>
        /// <param name="hashes">An instance of <see cref="Hashes"/>.</param>
        /// <returns>An instance of <see cref="LinkedDataModels"/>.</returns>
        LinkedDataModels AssignModelReferences(TrackInfo trackInfo, Hashes hashes);
    }
}