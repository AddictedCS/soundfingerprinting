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
    using SoundFingerprinting.Hashing;

    internal sealed class FingerprintCommand : ISourceFrom, IWithFingerprintConfiguration, IFingerprintCommand
    {
        private readonly IAudioService audioService;

        private readonly ILocalitySensitiveHashingAlgorithm lshAlgorithm;
        
        private readonly IFingerprintService fingerprintService;

        private Func<List<bool[]>> createFingerprintsMethod;

        private Func<List<FingerprintRawData>> createRawFingerprintsMethod;

        public FingerprintCommand(IFingerprintService fingerprintService, IAudioService audioService, ILocalitySensitiveHashingAlgorithm lshAlgorithm)
        {
            this.fingerprintService = fingerprintService;
            this.audioService = audioService;
            this.lshAlgorithm = lshAlgorithm;
        }

        public IFingerprintConfiguration FingerprintConfiguration { get; private set; }

        public Task<List<bool[]>> Fingerprint()
        {
            return Task.Factory.StartNew(createFingerprintsMethod);
        }

        public Task<List<FingerprintRawData>> FingerprintRaw()
        {
            return Task.Factory.StartNew(createRawFingerprintsMethod);
        }

        public Task<List<HashData>> Hash()
        {
            return Task.Factory
                .StartNew(createRawFingerprintsMethod)
                .ContinueWith(fingerprintsResult => HashFingerprints(fingerprintsResult.Result), TaskContinuationOptions.ExecuteSynchronously);
        }

        public IWithFingerprintConfiguration From(string pathToAudioFile)
        {
            createFingerprintsMethod = () =>
                {
                    float[] samples = audioService.ReadMonoFromFile(pathToAudioFile, FingerprintConfiguration.SampleRate, 0, 0);
                    return fingerprintService.CreateFingerprints(samples, FingerprintConfiguration);
                };

            createRawFingerprintsMethod = () => ExtractFingerprintRawDatas(FingerprintConfiguration, createFingerprintsMethod());

            return this;
        }

        public IWithFingerprintConfiguration From(float[] audioSamples)
        {
            createFingerprintsMethod = () => fingerprintService.CreateFingerprints(audioSamples, FingerprintConfiguration);
            createRawFingerprintsMethod = () => ExtractFingerprintRawDatas(FingerprintConfiguration, createFingerprintsMethod());
            return this;
        }

        public IWithFingerprintConfiguration From(IEnumerable<bool[]> fingerprints)
        {
            createFingerprintsMethod = fingerprints.ToList;
            createRawFingerprintsMethod = () => ExtractFingerprintRawDatas(FingerprintConfiguration, fingerprints.ToList());
            return this;
        }

        public IWithFingerprintConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond)
        {
            createFingerprintsMethod = () =>
                {
                    float[] samples = audioService.ReadMonoFromFile(pathToAudioFile, FingerprintConfiguration.SampleRate, secondsToProcess, startAtSecond);
                    return fingerprintService.CreateFingerprints(samples, FingerprintConfiguration);
                };

            createRawFingerprintsMethod = () => ExtractFingerprintRawDatas(FingerprintConfiguration, createFingerprintsMethod());

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
            var customFingerprintConfiguration = new CustomFingerprintConfiguration();
            FingerprintConfiguration = customFingerprintConfiguration;
            functor(customFingerprintConfiguration);
            return this;
        }

        public IFingerprintCommand WithDefaultFingerprintConfig()
        {
            FingerprintConfiguration = new DefaultFingerprintConfiguration();
            return this;
        }

        private List<FingerprintRawData> ExtractFingerprintRawDatas(IFingerprintConfiguration config, IEnumerable<bool[]> fingerprints)
        {
            var res = new List<FingerprintRawData>();
            int start = config.Stride.FirstStride / config.Overlap;

            for (int i = 0; i < fingerprints.Count(); i++)
            {
                var fingerprint = fingerprints.ElementAt(i);

                double begin = (double)config.Overlap * start / config.SampleRate;
                double end = (double)config.Overlap * (start + config.FingerprintLength) / config.SampleRate;

                start += config.FingerprintLength + (config.Stride.GetNextStride() / config.Overlap);

                var fingerprintRaw = new FingerprintRawData(fingerprint, begin, end);

                res.Add(fingerprintRaw);
            }

            return res;
        }

        private List<HashData> HashFingerprints(IEnumerable<FingerprintRawData> fingerprints)
        {
            var hashDatas = new ConcurrentBag<HashData>();
            Parallel.ForEach(
                fingerprints,
                fingerprint =>
                    {
                        var hashData = lshAlgorithm.Hash(
                            fingerprint.Fingerprint,
                            FingerprintConfiguration.NumberOfLSHTables,
                            FingerprintConfiguration.NumberOfMinHashesPerTable);

                        hashData.Begin = fingerprint.Begin;
                        hashData.End = fingerprint.End;
                        hashDatas.Add(hashData);
                    });

            return hashDatas.ToList();
        }
    }
}