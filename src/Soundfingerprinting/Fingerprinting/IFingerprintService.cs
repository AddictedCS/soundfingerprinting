namespace Soundfingerprinting.Fingerprinting
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;

    public interface IFingerprintService
    {
        Task<List<bool[]>> Process(WorkUnitParameterObject details);
    }
}