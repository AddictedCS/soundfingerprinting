namespace SoundFingerprinting.Hashing.LSH
{
    public interface ILSHService
    {
        long[] Hash(byte[] source, int numberOfHashTables, int numberOfHashesPerTable);
    }
}