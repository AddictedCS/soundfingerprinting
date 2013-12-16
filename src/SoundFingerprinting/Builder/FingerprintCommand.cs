namespace SoundFingerprinting.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Dao.Entities;
    using SoundFingerprinting.Hashing.LSH;
    using SoundFingerprinting.Hashing.MinHash;

    internal sealed class FingerprintCommand : ISourceFrom, IWithFingerprintConfiguration, IFingerprintCommand
    {
        private readonly IAudioService audioService;

        private readonly IMinHashService minHashService;

        private readonly ILSHService lshService;

        private readonly IFingerprintService fingerprintService;

        private Func<List<bool[]>> createFingerprintsMethod;

        private int trackId;

        public FingerprintCommand(IFingerprintService fingerprintService, IAudioService audioService, IMinHashService minHashService, ILSHService lshService)
        {
            this.fingerprintService = fingerprintService;
            this.audioService = audioService;
            this.minHashService = minHashService;
            this.lshService = lshService;
        }

        public IFingerprintingConfiguration Configuration { get; private set; }

        public IFingerprintCommand ForTrack(Track track)
        {
            trackId = track.Id;
            return this;
        }

        public Task<List<Fingerprint>> Fingerprint()
        {
            return Task.Factory.StartNew(createFingerprintsMethod).ContinueWith(fingerprintResult =>
                { 
                    var fingerprints = fingerprintResult.Result;
                    return AssociateFingerprintsWithTrack(fingerprints);
                });
        }

        public Task<List<SubFingerprint>> Encode()
        {
            return Task.Factory.StartNew(createFingerprintsMethod)
                .ContinueWith(fingerprintsResult => HashFingerprints(fingerprintsResult.Result))
                .ContinueWith(hashesResult => AssociateSubFingerprintsWithTrack(hashesResult.Result));
        }

        public Task<Dictionary<SubFingerprint, List<HashBinMinHash>>> Hash()
        {
            return Encode().ContinueWith(subFingerprintResult =>
                {
                    var subFingerprints = subFingerprintResult.Result;
                    Dictionary<SubFingerprint, List<HashBinMinHash>> hashes = new Dictionary<SubFingerprint, List<HashBinMinHash>>();
                    foreach (var subFingerprint in subFingerprints)
                    {
                        var hashBins = new List<HashBinMinHash>();
                        long[] groupedSubFingerprint = lshService.Hash(subFingerprint.Signature, Configuration.NumberOfLSHTables, Configuration.NumberOfMinHashesPerTable);
                        for (int i = 0; i < groupedSubFingerprint.Length; i++)
                        {
                            int tableNumber = i + 1;
                            hashBins.Add(new HashBinMinHash(groupedSubFingerprint[i], tableNumber, subFingerprint.Id));
                        }

                        hashes.Add(subFingerprint, hashBins);
                    }

                    return hashes;
                });
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

        private List<Fingerprint> AssociateFingerprintsWithTrack(IReadOnlyCollection<bool[]> rawFingerprints)
        {
            int fingerprintIndex = 0;
            return
                rawFingerprints.Select(
                    rawFingerprint =>
                    new Fingerprint(rawFingerprint, trackId, fingerprintIndex++, rawFingerprints.Count)).ToList();
        }

        private List<SubFingerprint> AssociateSubFingerprintsWithTrack(IEnumerable<byte[]> rawSubfingerprints)
        {
            return
                rawSubfingerprints.Select(rawSubfingerprint => new SubFingerprint(rawSubfingerprint, trackId)).ToList();
        }
    }
}