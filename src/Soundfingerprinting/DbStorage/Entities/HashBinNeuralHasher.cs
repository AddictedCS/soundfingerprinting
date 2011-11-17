// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;

namespace Soundfingerprinting.DbStorage.Entities
{
    /// <summary>
    ///   HashBinNeuralHasher entity object
    /// </summary>
    [Serializable]
    public class HashBinNeuralHasher : HashBin
    {
        /*Inherited for the sake of code verbosity*/

        /// <summary>
        ///   HashBinNeuralHasher class
        /// </summary>
        /// <param name = "id">Id of the hash bin</param>
        /// <param name = "hashBin">Hash bin itself</param>
        /// <param name = "hashTable">Hash table</param>
        /// <param name = "trackId">Track id</param>
        public HashBinNeuralHasher(int id, long hashBin, int hashTable, Int32 trackId) : base(id, hashBin, hashTable, trackId)
        {
        }
    }
}