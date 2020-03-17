﻿namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Command;

    public class QueryCommandBuilder : IQueryCommandBuilder
    {
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;

        public QueryCommandBuilder(): this(FingerprintCommandBuilder.Instance, QueryFingerprintService.Instance)
        {
        }

        internal QueryCommandBuilder(IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryFingerprintService = queryFingerprintService;
        }

        public IQuerySource BuildQueryCommand()
        {
            return new QueryCommand(fingerprintCommandBuilder, queryFingerprintService);
        }

        public IRealtimeSource BuildRealtimeQueryCommand()
        {
            return new RealtimeQueryCommand(fingerprintCommandBuilder, queryFingerprintService);
        }

        public static IQueryCommandBuilder Instance { get; } = new QueryCommandBuilder();
    }
}