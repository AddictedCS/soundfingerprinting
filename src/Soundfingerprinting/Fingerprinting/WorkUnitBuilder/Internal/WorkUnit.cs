namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal class WorkUnit : IWorkUnit
    {
        private WorkUnitParameterObject workUnitParameterObject;

        public WorkUnit(WorkUnitParameterObject workUnitParameterObject)
        {
            this.workUnitParameterObject = workUnitParameterObject;
        }

        public Task<List<bool[]>> GetFingerprintsUsingService(IFingerprintService service)
        {
            throw new NotImplementedException();
        }
    }
}