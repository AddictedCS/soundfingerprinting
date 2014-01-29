namespace SoundFingerprinting.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Dao.Entities;
    using SoundFingerprinting.Hashing.MinHash;

    internal sealed class AudioFingerprintingUnit : ISourceFrom, IWithAlgorithmConfiguration, IAudioFingerprintingUnit, IFingerprinter, IHasher
    {
        private readonly IAudioService audioService;

        private readonly IMinHashService minHashService;

        private readonly IFingerprintService fingerprintService;

        private CancellationToken cancellationToken;

        private Func<List<bool[]>> createFingerprintsMethod;

        public AudioFingerprintingUnit(IFingerprintService fingerprintService, IAudioService audioService, IMinHashService minHashService)
        {
            this.fingerprintService = fingerprintService;
            this.audioService = audioService;
            this.minHashService = minHashService;
        }

        public IFingerprintingConfiguration Configuration { get; private set; }

        public IFingerprinter FingerprintIt()
        {
            return this;
        }

        Task<List<bool[]>> IFingerprinter.AsIs()
        {
            return Task.Factory.StartNew(createFingerprintsMethod);
        }

        Task<List<byte[]>> IHasher.AsIs(CancellationToken token)
        {
            return Task.Factory.StartNew(createFingerprintsMethod, token).ContinueWith(task => HashFingerprints(task.Result));
        }

        Task<List<bool[]>> IFingerprinter.AsIs(CancellationToken token)
        {
            return Task.Factory.StartNew(createFingerprintsMethod, token);
        }

        Task<List<SubFingerprint>> IHasher.ForTrack(int trackId)
        {
            return ((IHasher)this).AsIs().ContinueWith(
                hashingResult => AssociateSubFingerprintsWithTrack(hashingResult.Result, trackId));
        }
        
        Task<List<SubFingerprint>> IHasher.ForTrack(int trackId, CancellationToken token)
        {
            return ((IHasher)this).AsIs(token).ContinueWith(
                hashingResult => AssociateSubFingerprintsWithTrack(hashingResult.Result, trackId));
        }

        Task<List<Fingerprint>> IFingerprinter.ForTrack(int trackId, CancellationToken token)
        {
            return ((IFingerprinter)this).AsIs(token).ContinueWith(
                fingerprintingResult => AssociateFingerprintsWithTrack(fingerprintingResult.Result, trackId));
        }

        Task<List<byte[]>> IHasher.AsIs()
        {
            return Task.Factory.StartNew(createFingerprintsMethod).ContinueWith(task => HashFingerprints(task.Result));
        }

        Task<List<Fingerprint>> IFingerprinter.ForTrack(int trackId)
        {
            return ((IFingerprinter)this).AsIs().ContinueWith(
                fingerprintingResult => AssociateFingerprintsWithTrack(fingerprintingResult.Result, trackId));
        }
        
        public IHasher HashIt()
        {
            return this;
        }

        public Task<List<bool[]>> RunAlgorithm(CancellationToken token)
        {
            return Task.Factory.StartNew(createFingerprintsMethod, token);
        }

        public Task<List<byte[]>> RunAlgorithmWithHashing()
        {
            return Task.Factory.StartNew(createFingerprintsMethod).ContinueWith(task => HashFingerprints(task.Result));
        }

        public Task<List<byte[]>> RunAlgorithmWithHashing(CancellationToken token)
        {
            cancellationToken = token;
            return Task.Factory.StartNew(createFingerprintsMethod, token).ContinueWith(task => HashFingerprints(task.Result));
        }

        public IWithAlgorithmConfiguration From(string pathToAudioFile)
        {
            createFingerprintsMethod = () =>
            {
                float[] samples = audioService.ReadMonoFromFile(pathToAudioFile, Configuration.SampleRate, 0, 0);
                if (cancellationToken.IsCancellationRequested)
                {
                    return new List<bool[]>(Enumerable.Empty<bool[]>());
                }

                return fingerprintService.CreateFingerprints(samples, Configuration);
            };

            return this;
        }

        public IWithAlgorithmConfiguration From(float[] audioSamples)
        {
            createFingerprintsMethod = () => fingerprintService.CreateFingerprints(audioSamples, Configuration);
            return this;
        }

        public IWithAlgorithmConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond)
        {
            createFingerprintsMethod = () =>
                {
                    float[] samples = audioService.ReadMonoFromFile(pathToAudioFile, Configuration.SampleRate, secondsToProcess, startAtSecond);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return new List<bool[]>(Enumerable.Empty<bool[]>());
                    }

                    return fingerprintService.CreateFingerprints(samples, Configuration);
                };
            return this;
        }

        public IAudioFingerprintingUnit WithAlgorithmConfiguration(IFingerprintingConfiguration configuration)
        {
            Configuration = configuration;
            return this;
        }

        public IAudioFingerprintingUnit WithAlgorithmConfiguration<T>() where T : IFingerprintingConfiguration, new()
        {
            Configuration = new T();
            return this;
        }

        public IAudioFingerprintingUnit WithCustomAlgorithmConfiguration(Action<CustomFingerprintingConfiguration> functor)
        {
            CustomFingerprintingConfiguration customFingerprintingConfiguration = new CustomFingerprintingConfiguration();
            Configuration = customFingerprintingConfiguration;
            functor(customFingerprintingConfiguration);
            return this;
        }

        public IAudioFingerprintingUnit WithDefaultAlgorithmConfiguration()
        {
            Configuration = new DefaultFingerprintingConfiguration();
            return this;
        }

        private List<byte[]> HashFingerprints(IEnumerable<bool[]> fingerprints)
        {
            List<byte[]> subFingerprints = new List<byte[]>();
            foreach (var fingerprint in fingerprints)
            {
                subFingerprints.Add(minHashService.Hash(fingerprint));
            }

            return subFingerprints;
        }

        private List<Fingerprint> AssociateFingerprintsWithTrack(IReadOnlyCollection<bool[]> rawFingerprints, int trackId)
        {
            List<Fingerprint> fingerprints = new List<Fingerprint>();
            int fingerprintIndex = 0;
            foreach (var rawFingerprint in rawFingerprints)
            {
                fingerprints.Add(new Fingerprint(rawFingerprint, trackId, fingerprintIndex++, rawFingerprints.Count));
            }

            return fingerprints;
        }

        private List<SubFingerprint> AssociateSubFingerprintsWithTrack(IEnumerable<byte[]> rawSubfingerprints, int trackId)
        {
            List<SubFingerprint> subFingerprints = new List<SubFingerprint>();
            foreach (var rawSubfingerprint in rawSubfingerprints)
            {
                subFingerprints.Add(new SubFingerprint(rawSubfingerprint, trackId));
            }

            return subFingerprints;
        }
    }
}