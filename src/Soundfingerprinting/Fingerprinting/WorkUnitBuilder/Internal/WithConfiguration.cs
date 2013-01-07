namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder.Internal
{
    using System;

    using Soundfingerprinting.Fingerprinting.Configuration;

    internal class WithConfiguration : IWithConfiguration
    {
        private readonly WorkUnitParameterObject workUnitParameterObject;

        public WithConfiguration(WorkUnitParameterObject workUnitParameterObject)
        {
            this.workUnitParameterObject = workUnitParameterObject;
        }

        public IWorkUnit With(IFingerprintingConfiguration configuration)
        {
            workUnitParameterObject.FingerprintingConfiguration = configuration;
            return new WorkUnit(workUnitParameterObject);
        }

        public IWorkUnit With<T>() where T : IFingerprintingConfiguration, new()
        {
            workUnitParameterObject.FingerprintingConfiguration = new T();
            return new WorkUnit(workUnitParameterObject);
        }

        public IWorkUnit WithCustomConfiguration(Action<CustomFingerprintingConfiguration> transformation)
        {
            CustomFingerprintingConfiguration customFingerprintingConfiguration = new CustomFingerprintingConfiguration();
            workUnitParameterObject.FingerprintingConfiguration = customFingerprintingConfiguration;
            transformation(customFingerprintingConfiguration);
            return new WorkUnit(workUnitParameterObject);
        }
    }
}