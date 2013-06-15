namespace SoundFingerprinting
{
    using SoundFingerprinting.Query;

    public interface IFingerprintQueryBuilder
    {
        IQuerySource BuildQuery();
    }
}