namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Soundfingerprinting.Fingerprinting.Configuration;

    public interface IFingerprintingUnit
    {
        IFingerprintingConfiguration Configuration { get; }

        Task<List<bool[]>> RunAlgorithm();

        Task<List<bool[]>> RunAlgorithm(CancellationToken token);

        Task<List<byte[]>> RunAlgorithmWithHashing();

        Task<List<byte[]>> RunAlgorithmWithHashing(CancellationToken token);
    }
}