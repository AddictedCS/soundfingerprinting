namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Command;

    public class FingerprintCommandBuilder : IFingerprintCommandBuilder
    {
        private readonly IFingerprintService fingerprintService;

        public FingerprintCommandBuilder(): this(FingerprintService.Instance)
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

        public static IFingerprintCommandBuilder Instance { get; } = new FingerprintCommandBuilder();
    }
}