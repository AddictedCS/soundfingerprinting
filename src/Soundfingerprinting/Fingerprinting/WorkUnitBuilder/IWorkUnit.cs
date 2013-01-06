namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IWorkUnit
    {
        Task<List<bool[]>> GetFingerprintsUsingService(IFingerprintService service);
    }
}