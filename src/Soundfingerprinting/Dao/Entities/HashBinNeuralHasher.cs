namespace Soundfingerprinting.DbStorage.Entities
{
    using System;

    [Serializable]
    public class HashBinNeuralHasher : HashBin
    {
        public HashBinNeuralHasher(int id, long hashBin, int hashTable, int trackId) : base(id, hashBin, hashTable, trackId)
        {
        }
    }
}