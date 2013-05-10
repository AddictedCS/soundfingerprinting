namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Soundfingerprinting.Fingerprinting.Configuration;

    public interface IFingerprintingUnit
    {
        IFingerprintingConfiguration Configuration { get; }

        Task<List<bool[]>> RunAlgorithm();

        Task<List<byte[]>> RunAlgorithmWithHashing();
    }
}