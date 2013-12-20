namespace SoundFingerprinting.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
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

        public IFingerprintingConfiguration Configuration { get; private set; }

        public Task<List<bool[]>> Fingerprint()
        {
            return Task.Factory.StartNew(createFingerprintsMethod).ContinueWith(task => task.Result);
        }

        public Task<List<HashData>> Hash()
        {
            return Task.Factory
                .StartNew(createFingerprintsMethod)
                .ContinueWith(fingerprintsResult => HashFingerprints(fingerprintsResult.Result))
                .ContinueWith(subFingerprintResult => GroupSubFingerprintsIntoBuckets(subFingerprintResult.Result));
        }

        public IWithFingerprintConfiguration From(string pathToAudioFile)
        {
            createFingerprintsMethod = () =>
                {
                    float[] samples = audioService.ReadMonoFromFile(pathToAudioFile, Configuration.SampleRate, 0, 0);
                    return fingerprintService.CreateFingerprints(samples, Configuration);
                };

            return this;
        }

        public IWithFingerprintConfiguration From(float[] audioSamples)
        {
            createFingerprintsMethod = () => fingerprintService.CreateFingerprints(audioSamples, Configuration);
            return this;
        }

        public IWithFingerprintConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond)
        {
            createFingerprintsMethod = () =>
                {
                    float[] samples = audioService.ReadMonoFromFile(pathToAudioFile, Configuration.SampleRate, secondsToProcess, startAtSecond);
                    return fingerprintService.CreateFingerprints(samples, Configuration);
                };

            return this;
        }

        public IFingerprintCommand WithAlgorithmConfiguration(IFingerprintingConfiguration configuration)
        {
            Configuration = configuration;
            return this;
        }

        public IFingerprintCommand WithAlgorithmConfiguration<T>() where T : IFingerprintingConfiguration, new()
        {
            Configuration = new T();
            return this;
        }

        public IFingerprintCommand WithCustomAlgorithmConfiguration(Action<CustomFingerprintingConfiguration> functor)
        {
            CustomFingerprintingConfiguration customFingerprintingConfiguration = new CustomFingerprintingConfiguration();
            Configuration = customFingerprintingConfiguration;
            functor(customFingerprintingConfiguration);
            return this;
        }

        public IFingerprintCommand WithDefaultAlgorithmConfiguration()
        {
            Configuration = new DefaultFingerprintingConfiguration();
            return this;
        }

        private List<byte[]> HashFingerprints(IEnumerable<bool[]> fingerprints)
        {
            List<byte[]> subFingerprints = new List<byte[]>();
            Parallel.ForEach(fingerprints, fingerprint => subFingerprints.Add(minHashService.Hash(fingerprint)));
            return subFingerprints;
        }

        private List<HashData> GroupSubFingerprintsIntoBuckets(IEnumerable<byte[]> subFingerprints)
        {
            List<HashData> hashDatas = new List<HashData>();
            Parallel.ForEach(
                subFingerprints,
                subFingerprint =>
                    {
                        long[] groupedSubFingerprint = lshService.Hash(
                            subFingerprint, Configuration.NumberOfLSHTables, Configuration.NumberOfMinHashesPerTable);
                        hashDatas.Add(new HashData(subFingerprint, groupedSubFingerprint));
                    });

            return hashDatas;
        }
    }
}