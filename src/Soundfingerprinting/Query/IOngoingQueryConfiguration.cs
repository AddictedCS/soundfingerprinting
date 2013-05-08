namespace Soundfingerprinting.Query
{
    public interface IOngoingQueryConfiguration
    {
        IFingerprintingQueryUnit With(IQueryConfiguration queryConfiguration);
    }
}