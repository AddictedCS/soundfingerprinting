namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Soundfingerprinting.Fingerprinting.Configuration;

    public interface IWorkUnit
    {
        IFingerprintingConfiguration Configuration { get; }

        Task<List<bool[]>> GetFingerprintsUsingService(IFingerprintService service);
    }
}