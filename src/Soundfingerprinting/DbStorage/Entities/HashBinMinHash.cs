// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;

namespace Soundfingerprinting.DbStorage.Entities
{
    /// <summary>
    ///   HashBin for Min-Hash + LSH schema
    /// </summary>
    [Serializable]
    public class HashBinMinHash : HashBin
    {
        #region Constructors

        /// <summary>
        ///   HashBin's constructor
        /// </summary>
        /// <param name = "id">Id of the fingerprint</param>
        /// <param name = "hashBin">Actual hash bin</param>
        /// <param name = "hashTable">Hash table to which the hash bin is belonging to</param>
        /// <param name = "trackId">Track's identifier</param>
        /// <param name = "hashedFingerprint">HashedFingerprint id</param>
        public HashBinMinHash(int id, long hashBin, int hashTable, Int32 trackId, Int32 hashedFingerprint)
            : base(id, hashBin, hashTable, trackId)
        {
            HashedFingerprint = hashedFingerprint;
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Hashed fingerprint
        /// </summary>
        public Int32 HashedFingerprint { get; set; }

        #endregion
    }
}