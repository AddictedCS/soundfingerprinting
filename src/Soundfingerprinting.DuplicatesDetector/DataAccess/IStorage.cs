// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System.Collections.Generic;
using Soundfingerprinting.DuplicatesDetector.Model;

namespace Soundfingerprinting.DuplicatesDetector.DataAccess
{
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
        /// <param name = "type">Type of the hash</param>
        void InsertHash(HashSignature hash, HashType type);

        /// <summary>
        ///   Get tracks that correspond to a specific signature, with specified threshold
        /// </summary>
        /// <param name = "hashSignature">Hash signature</param>
        /// <param name = "hashTableThreshold">Hash threshold</param>
        /// <returns>Tracks that correspond to the hash</returns>
        Dictionary<Track, int> GetTracks(int[] hashSignature, int hashTableThreshold);

        /// <summary>
        ///   Get all hash signatures from a specific track
        /// </summary>
        /// <param name = "track">Inquired track</param>
        /// <param name = "type">Hash type</param>
        /// <returns>Hash signatures</returns>
        HashSet<HashSignature> GetHashSignatures(Track track, HashType type);

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