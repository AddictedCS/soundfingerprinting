namespace Soundfingerprinting.Query
{
    using Soundfingerprinting.Query.Configuration;

    public interface IOngoingQueryConfiguration
    {
        IFingerprintingQueryUnit With(IQueryConfiguration queryConfiguration);
    }
}