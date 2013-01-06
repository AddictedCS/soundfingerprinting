namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder
{
    using System;

    using Soundfingerprinting.Fingerprinting.Configuration;

    public interface IWithConfiguration
    {
        IWorkUnit With(IFingerprintingConfig configuration);

        IWorkUnit With<T>() where T : IFingerprintingConfig, new();

        IWorkUnit WithCustomConfiguration(Action<CustomFingerprintingConfig> transformation);
    }
}