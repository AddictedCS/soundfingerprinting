namespace SoundFingerprinting.Builder
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using SoundFingerprinting.Command;

    /// <summary>
    ///  Fingerprint command builder.
    /// </summary>
    public sealed class FingerprintCommandBuilder : IFingerprintCommandBuilder
    {
        private readonly ILoggerFactory? loggerFactory;
        private readonly IFingerprintService fingerprintService;

        /// <summary>
        ///  Initializes a new instance of the <see cref="FingerprintCommandBuilder"/> class.
        /// </summary>
        /// <param name="loggerFactory">Logger factory.</param>
        public FingerprintCommandBuilder(ILoggerFactory? loggerFactory = null) : this(FingerprintService.Instance)
        {
            this.loggerFactory = loggerFactory;
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
            return new FingerprintCommand(fingerprintService, loggerFactory ?? new NullLoggerFactory());
        }
    }
}