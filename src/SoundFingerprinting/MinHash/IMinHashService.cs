namespace SoundFingerprinting.MinHash
{
    internal interface IMinHashService
    {
        int PermutationsCount { get; }

        byte[] Hash(bool[] fingerprint);
    }
}