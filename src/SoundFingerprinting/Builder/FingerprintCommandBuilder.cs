namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Command;

    /// <summary>
    ///  Fingerprint command builder.
    /// </summary>
    public sealed class FingerprintCommandBuilder : IFingerprintCommandBuilder
    {
        private readonly IFingerprintService fingerprintService;

        /// <summary>
        ///  Initializes a new instance of the <see cref="FingerprintCommandBuilder"/> class.
        /// </summary>
        public FingerprintCommandBuilder() : this(FingerprintService.Instance)
        {
        }

        internal FingerprintCommandBuilder(IFingerprintService fingerprintService)
        {
            this.fingerprintService = fingerprintService;
        }

        /// <summary>
        ///  Gets an instance of the <see cref="FingerprintCommandBuilder"/> class.
        /// </summary>
        public static IFingerprintCommandBuilder Instance { get; } = new FingerprintCommandBuilder();

        /// <inheritdoc cref="IFingerprintCommandBuilder"/>
        public ISourceFrom BuildFingerprintCommand()
        {
            return new FingerprintCommand(fingerprintService);
        }
    }
}