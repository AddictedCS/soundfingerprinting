namespace Soundfingerprinting.Hashing.MinHash
{
    public interface IMinHashService
    {
        int PermutationsCount { get; }

        byte[] Hash(bool[] fingerprint);
    }
}