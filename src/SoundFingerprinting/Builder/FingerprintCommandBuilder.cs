namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Infrastructure;

    public class FingerprintCommandBuilder : IFingerprintCommandBuilder
    {
        private readonly IFingerprintService fingerprintService;

        public FingerprintCommandBuilder() : this(DependencyResolver.Current.Get<IFingerprintService>())
        {
        }

        internal FingerprintCommandBuilder(IFingerprintService fingerprintService)
        {
            this.fingerprintService = fingerprintService;
        }

        public ISourceFrom BuildFingerprintCommand()
        {
            return new FingerprintCommand(fingerprintService);
        }
    }
}