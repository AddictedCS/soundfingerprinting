namespace Soundfingerprinting.DuplicatesDetector.DataAccess
{
    using System.Collections.Generic;

    using Soundfingerprinting.DuplicatesDetector.Model;

    /// <summary>
    ///   Storage used for hashes and tracks
    /// </summary>
    public interface IStorage
    {
        /// <summary>
        ///   Insert track
        /// </summary>
        /// <param name = "track">Track to be inserted</param>
        void InsertTrack(Track track);

        /// <summary>
        ///   Remove track
        /// </summary>
        /// <param name = "track">Track to be removed</param>
        void RemoveTrack(Track track);

        /// <summary>
        ///   Insert hash
        /// </summary>
        /// <param name = "hash">Hash to be inserted</param>
        void InsertHash(HashSignature hash);

        /// <summary>
        ///   Get tracks that correspond to a specific signature, with specified threshold
        /// </summary>
        /// <param name = "hashSignature">Hash signature</param>
        /// <param name = "hashTableThreshold">Hash threshold</param>
        /// <returns>Tracks that correspond to the hash</returns>
        Dictionary<Track, int> GetTracks(HashSignature hashSignature, int hashTableThreshold);

        /// <summary>
        ///   Get all hash signatures from a specific track
        /// </summary>
        /// <param name = "track">Inquired track</param>
        /// <returns>Hash signatures</returns>
        HashSet<HashSignature> GetHashSignatures(Track track);

        /// <summary>
        ///   Get all tracks from the storage
        /// </summary>
        /// <returns>All tracks in the storage</returns>
        List<Track> GetAllTracks();

        /// <summary>
        ///   Clear all data from the storage
        /// </summary>
        void ClearAll();
    }
}