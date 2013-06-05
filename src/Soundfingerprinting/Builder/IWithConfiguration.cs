namespace Soundfingerprinting.Builder
{
    using System;

    using Soundfingerprinting.Configuration;

    public interface IWithConfiguration
    {
        IFingerprintUnit With(IFingerprintingConfiguration configuration);

        IFingerprintUnit With<T>() where T : IFingerprintingConfiguration, new();

        IFingerprintUnit WithCustomConfiguration(Action<CustomFingerprintingConfiguration> transformation);
    }
}