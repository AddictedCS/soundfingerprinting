namespace SoundFingerprinting.Builder
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using SoundFingerprinting.Configuration;

    public interface IFingerprintUnit
    {
        IFingerprintingConfiguration Configuration { get; }

        Task<List<bool[]>> RunAlgorithm();

        Task<List<bool[]>> RunAlgorithm(CancellationToken token);

        Task<List<byte[]>> RunAlgorithmWithHashing();

        Task<List<byte[]>> RunAlgorithmWithHashing(CancellationToken token);
    }
}