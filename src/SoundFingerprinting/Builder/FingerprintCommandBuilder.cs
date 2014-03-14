namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.LSH;

    public class FingerprintCommandBuilder : IFingerprintCommandBuilder
    {
        private readonly IFingerprintService fingerprintService;
        private readonly ILocalitySensitiveHashingAlgorithm lshAlgorithm;

        public FingerprintCommandBuilder()
            : this(
                DependencyResolver.Current.Get<IFingerprintService>(),
                DependencyResolver.Current.Get<ILocalitySensitiveHashingAlgorithm>())
        {
        }

        internal FingerprintCommandBuilder(IFingerprintService fingerprintService, ILocalitySensitiveHashingAlgorithm lshAlgorithm)
        {
            this.fingerprintService = fingerprintService;
            this.lshAlgorithm = lshAlgorithm;
        }

        public ISourceFrom BuildFingerprintCommand()
        {
            return new FingerprintCommand(fingerprintService, lshAlgorithm);
        }
    }
}