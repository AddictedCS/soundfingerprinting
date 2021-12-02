namespace SoundFingerprinting.Builder
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using SoundFingerprinting.Command;

    /// <summary>
    ///  Query command builder.
    /// </summary>
    public class QueryCommandBuilder : IQueryCommandBuilder
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;

        /// <summary>
        ///  Gets an instance of <see cref="QueryCommandBuilder"/>.
        /// </summary>
        public static IQueryCommandBuilder Instance { get; } = new QueryCommandBuilder();
        
        /// <summary>
        ///  Initializes a new instance of the <see cref="QueryCommandBuilder"/> class.
        /// </summary>
        /// <param name="loggerFactory">Instance of <see cref="ILoggerFactory"/> interface.</param>
        public QueryCommandBuilder(ILoggerFactory? loggerFactory = null) : this(FingerprintCommandBuilder.Instance, QueryFingerprintService.Instance, loggerFactory ?? new NullLoggerFactory())
        {
        }

        internal QueryCommandBuilder(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService, ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryFingerprintService = queryFingerprintService;
        }

        /// <inheritdoc cref="IQueryCommandBuilder.BuildQueryCommand"/>
        public IQuerySource BuildQueryCommand()
        {
            return new QueryCommand(fingerprintCommandBuilder, queryFingerprintService);
        }

        /// <inheritdoc cref="IQueryCommandBuilder.BuildRealtimeQueryCommand"/>
        public IRealtimeSource BuildRealtimeQueryCommand()
        {
            return new RealtimeQueryCommand(fingerprintCommandBuilder, queryFingerprintService, loggerFactory);
        }
    }
}