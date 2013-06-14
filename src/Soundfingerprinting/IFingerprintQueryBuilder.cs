namespace SoundFingerprinting
{
    using SoundFingerprinting.Query;

    public interface IFingerprintQueryBuilder
    {
        IOngoingQuery BuildQuery();
    }
}