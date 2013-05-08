namespace Soundfingerprinting.Query
{
    using Soundfingerprinting.Fingerprinting.Configuration;

    public interface IOngoingQueryConfigurationWithFingerprinting
    {
        IFingerprintingQueryUnit With(
            IFingerprintingConfiguration fingerprintingConfiguration, IQueryConfiguration queryConfiguration);
    }
}