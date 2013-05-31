namespace Soundfingerprinting.Fingerprinting.FingerprintUnitBuilder
{
    using System;

    using Soundfingerprinting.Fingerprinting.Configuration;

    public interface IWithConfiguration
    {
        IFingerprintingUnit With(IFingerprintingConfiguration configuration);

        IFingerprintingUnit With<T>() where T : IFingerprintingConfiguration, new();

        IFingerprintingUnit WithCustomConfiguration(Action<CustomFingerprintingConfiguration> transformation);
    }
}