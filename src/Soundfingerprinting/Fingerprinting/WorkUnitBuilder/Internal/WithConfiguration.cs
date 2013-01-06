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

        public IWorkUnit With(IFingerprintingConfig configuration)
        {
            workUnitParameterObject.FingerprintingConfig = configuration;
            return new WorkUnit(workUnitParameterObject);
        }

        public IWorkUnit With<T>() where T : IFingerprintingConfig, new()
        {
            workUnitParameterObject.FingerprintingConfig = new T();
            return new WorkUnit(workUnitParameterObject);
        }

        public IWorkUnit WithCustomConfiguration(Action<CustomFingerprintingConfig> transformation)
        {
            CustomFingerprintingConfig customFingerprintingConfig = new CustomFingerprintingConfig();
            workUnitParameterObject.FingerprintingConfig = customFingerprintingConfig;
            transformation(customFingerprintingConfig);
            return new WorkUnit(workUnitParameterObject);
        }
    }
}