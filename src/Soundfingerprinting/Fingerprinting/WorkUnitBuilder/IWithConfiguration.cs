namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder
{
    using System;

    using Soundfingerprinting.Fingerprinting.Configuration;

    public interface IWithConfiguration
    {
        IWorkUnit With(IFingerprintingConfiguration configuration);

        IWorkUnit With<T>() where T : IFingerprintingConfiguration, new();

        IWorkUnit WithCustomConfiguration(Action<CustomFingerprintingConfiguration> transformation);
    }
}