namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    public class RealtimeQueryCommand : IRealtimeSource, IWithRealtimeQueryConfiguration, IUsingRealtimeQueryServices, IRealtimeQueryCommand
    {
        private const int MinSamplesForOneFingerprint = 10240;
        private const int SupportedFrequency = 5512;
        private const int MillisecondsDelay = (int)((double) MinSamplesForOneFingerprint / SupportedFrequency * 1000);

        private BlockingCollection<AudioSamples> realtimeSamples;
        private RealtimeQueryConfiguration configuration;
        private IModelService modelService;
        private IAudioService audioService;

        public RealtimeQueryCommand()
        {
            configuration = new DefaultRealtimeQueryConfiguration(
                e => throw new Exception("Register a success callback for your realtime query"), 
                e => { /* do nothing */ }, fingerprints => { /* do nothing */ });
        }
        
        public IWithRealtimeQueryConfiguration From(BlockingCollection<AudioSamples> audioSamples)
        {
            realtimeSamples = audioSamples;
            return this;
        }

        public IUsingRealtimeQueryServices WithRealtimeQueryConfig(RealtimeQueryConfiguration realtimeQueryConfiguration)
        {
            configuration = realtimeQueryConfiguration;
            return this;
        }

        public IUsingRealtimeQueryServices WithRealtimeQueryConfig(Func<RealtimeQueryConfiguration, RealtimeQueryConfiguration> amendQueryFunctor)
        {
            configuration = amendQueryFunctor(configuration);
            return this;
        }

        public async Task<double> Query(CancellationToken cancellationToken)
        {
            var realtimeSamplesAggregator = new RealtimeAudioSamplesAggregator(configuration.Stride, MinSamplesForOneFingerprint);
            var resultsAggregator = new StatefulRealtimeResultEntryAggregator(configuration.ResultEntryFilter, configuration.PermittedGap);
            var commandBuilder = FingerprintCommandBuilder.Instance;
            var queryFingerprintService = QueryFingerprintService.Instance;

            double queryLength = 0d;
            while (!realtimeSamples.IsAddingCompleted && !cancellationToken.IsCancellationRequested)
            {
                AudioSamples audioSamples;
                try
                {
                    if (!realtimeSamples.TryTake(out audioSamples, MillisecondsDelay, cancellationToken))
                    {
                        continue;
                    }
                }
                catch (OperationCanceledException)
                {
                    return queryLength;
                }

                if (audioSamples.SampleRate != SupportedFrequency)
                {
                    throw new ArgumentException($"{nameof(audioSamples)} should be provided down sampled to {SupportedFrequency}Hz");
                }

                queryLength += audioSamples.Duration;

                var prefixed = realtimeSamplesAggregator.Aggregate(audioSamples);
                var hashes = await CreateQueryFingerprints(commandBuilder, prefixed);
                var results = queryFingerprintService.Query(hashes, configuration.QueryConfiguration, modelService);
                var realtimeQueryResult = resultsAggregator.Consume(results.ResultEntries, audioSamples.Duration);

                InvokeSuccessHandler(realtimeQueryResult);
                InvokeDidNotPassFilterHandler(realtimeQueryResult);
                InvokeHashedFingerprintsCallback(hashes);
            }

            return queryLength;
        }

        private void InvokeHashedFingerprintsCallback(List<HashedFingerprint> hashes)
        {
            configuration?.QueryFingerprintsCallback(hashes);
        }

        private async Task<List<HashedFingerprint>> CreateQueryFingerprints(IFingerprintCommandBuilder commandBuilder, AudioSamples prefixed)
        {
            return await commandBuilder.BuildFingerprintCommand()
                .From(prefixed)
                .WithFingerprintConfig(configuration.QueryConfiguration.FingerprintConfiguration)
                .UsingServices(audioService)
                .Hash();
        }

        private void InvokeDidNotPassFilterHandler(RealtimeQueryResult realtimeQueryResult)
        {
            foreach (var result in realtimeQueryResult.DidNotPassThresholdEntries)
            {
                configuration?.DidNotPassFilterCallback(result);
            }
        }

        private void InvokeSuccessHandler(RealtimeQueryResult realtimeQueryResult)
        {
            foreach (var result in realtimeQueryResult.SuccessEntries)
            {
                configuration?.SuccessCallback(result);
            }
        }

        public IRealtimeQueryCommand UsingServices(IModelService modelService)
        {
            this.modelService = modelService;
            audioService = new SoundFingerprintingAudioService();
            return this;
        }
    }
}