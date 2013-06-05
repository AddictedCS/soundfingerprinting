namespace Soundfingerprinting
{
    using Soundfingerprinting.Query;

    public interface IFingerprintQueryBuilder
    {
        IOngoingQuery BuildQuery();
    }
}