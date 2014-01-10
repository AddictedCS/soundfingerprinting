namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Hashing.LSH;
    using SoundFingerprinting.Hashing.MinHash;

    internal sealed class FingerprintCommand : ISourceFrom, IWithFingerprintConfiguration, IFingerprintCommand
    {
        private readonly IAudioService audioService;

        private readonly IMinHashService minHashService;

        private readonly ILSHService lshService;

        private readonly IFingerprintService fingerprintService;

        private Func<List<bool[]>> createFingerprintsMethod;

        public FingerprintCommand(IFingerprintService fingerprintService, IAudioService audioService, IMinHashService minHashService, ILSHService lshService)
        {
            this.fingerprintService = fingerprintService;
            this.audioService = audioService;
            this.minHashService = minHashService;
            this.lshService = lshService;
        }

        public IFingerprintConfiguration FingerprintConfiguration { get; private set; }

        public Task<List<bool[]>> Fingerprint()
        {
            return Task.Factory.StartNew(createFingerprintsMethod);
        }

        public Task<List<HashData>> Hash()
        {
            return Task.Factory
                .StartNew(createFingerprintsMethod)
                .ContinueWith(fingerprintsResult => HashFingerprints(fingerprintsResult.Result), TaskContinuationOptions.ExecuteSynchronously)
                .ContinueWith(subFingerprintResult => GroupSubFingerprintsIntoBuckets(subFingerprintResult.Result), TaskContinuationOptions.ExecuteSynchronously);
        }

        public IWithFingerprintConfiguration From(string pathToAudioFile)
        {
            createFingerprintsMethod = () =>
                {
                    float[] samples = audioService.ReadMonoFromFile(pathToAudioFile, FingerprintConfiguration.SampleRate, 0, 0);
                    return fingerprintService.CreateFingerprints(samples, FingerprintConfiguration);
                };

            return this;
        }

        public IWithFingerprintConfiguration From(float[] audioSamples)
        {
            createFingerprintsMethod = () => fingerprintService.CreateFingerprints(audioSamples, FingerprintConfiguration);
            return this;
        }

        public IWithFingerprintConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond)
        {
            createFingerprintsMethod = () =>
                {
                    float[] samples = audioService.ReadMonoFromFile(pathToAudioFile, FingerprintConfiguration.SampleRate, secondsToProcess, startAtSecond);
                    return fingerprintService.CreateFingerprints(samples, FingerprintConfiguration);
                };

            return this;
        }

        public IFingerprintCommand WithFingerprintConfig(IFingerprintConfiguration configuration)
        {
            FingerprintConfiguration = configuration;
            return this;
        }

        public IFingerprintCommand WithFingerprintConfig<T>() where T : IFingerprintConfiguration, new()
        {
            FingerprintConfiguration = new T();
            return this;
        }

        public IFingerprintCommand WithFingerprintConfig(Action<CustomFingerprintConfiguration> functor)
        {
            CustomFingerprintConfiguration customFingerprintConfiguration = new CustomFingerprintConfiguration();
            FingerprintConfiguration = customFingerprintConfiguration;
            functor(customFingerprintConfiguration);
            return this;
        }

        public IFingerprintCommand WithDefaultFingerprintConfig()
        {
            FingerprintConfiguration = new DefaultFingerprintConfiguration();
            return this;
        }

        private List<byte[]> HashFingerprints(IEnumerable<bool[]> fingerprints)
        {
            var subFingerprints = new ConcurrentBag<byte[]>();
            Parallel.ForEach(fingerprints, fingerprint => subFingerprints.Add(minHashService.Hash(fingerprint)));
            return subFingerprints.ToList();
        }

        private List<HashData> GroupSubFingerprintsIntoBuckets(IEnumerable<byte[]> subFingerprints)
        {
            var hashDatas = new ConcurrentBag<HashData>();
            Parallel.ForEach(
                subFingerprints,
                subFingerprint =>
                    {
                        long[] groupedSubFingerprint = lshService.Hash(
                            subFingerprint, FingerprintConfiguration.NumberOfLSHTables, FingerprintConfiguration.NumberOfMinHashesPerTable);
                        hashDatas.Add(new HashData(subFingerprint, groupedSubFingerprint));
                    });

            return hashDatas.ToList();
        }
    }
}