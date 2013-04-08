namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder.Internal
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Soundfingerprinting.Fingerprinting.Configuration;

    internal class WorkUnit : IWorkUnit
    {
        private readonly WorkUnitParameterObject workUnitParameterObject;

        public WorkUnit(WorkUnitParameterObject workUnitParameterObject)
        {
            this.workUnitParameterObject = workUnitParameterObject;
        }

        public IFingerprintingConfiguration Configuration
        {
            get
            {
                return workUnitParameterObject.FingerprintingConfiguration;
            }
        }

        public Task<List<bool[]>> GetFingerprintsUsingService(IFingerprintService service)
        {
            return service.Process(workUnitParameterObject);
        }
    }
}