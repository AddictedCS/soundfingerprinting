// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;

namespace Soundfingerprinting.DbStorage.Entities
{
    /// <summary>
    ///   HashBin entity object
    /// </summary>
    [Serializable]
    public class HashBin
    {
        #region Constructors

        /// <summary>
        ///   Parameter less Constructor
        /// </summary>
        public HashBin()
        {
            Id = Int32.MinValue;
        }

        /// <summary>
        ///   HashBin constructor
        /// </summary>
        /// <param name = "id">Id of the fingerprint</param>
        /// <param name = "hashBin">Actual hash bin</param>
        /// <param name = "hashTable">Hash table to which the hash bin is belonging to</param>
        /// <param name = "trackId">Track's identifier</param>
        public HashBin(int id, long hashBin, int hashTable, Int32 trackId)
        {
            Id = id;
            Hashbin = hashBin;
            HashTable = hashTable;
            TrackId = trackId;
        }

        #endregion

        #region Properties

        /// <summary>
        ///   HashBin's id
        /// </summary>
        /// <remarks>
        ///   Once inserted into the database the object will be given a unique identifier
        /// </remarks>
        public Int32 Id { get; set; }

        /// <summary>
        ///   The actual hash bin
        /// </summary>
        public long Hashbin { get; set; }

        /// <summary>
        ///   HashBin's hash table
        /// </summary>
        public int HashTable { get; set; }

        /// <summary>
        ///   Track's Id, to which the HashBinNeuralHasher is pointing
        /// </summary>
        public Int32 TrackId { get; set; }

        #endregion
    }
}