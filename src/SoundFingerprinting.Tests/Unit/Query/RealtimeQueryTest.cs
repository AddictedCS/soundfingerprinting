namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Strides;

    [TestFixture]
    public class RealtimeQueryTest
    {
        [Test]
        public async Task RealtimeQueryShouldMatchOnlySelectedClusters()
        {
            var audioService = new SoundFingerprintingAudioService();
            var modelService = new InMemoryModelService();
            int minSize = 8192 + 2048;
            int count = 10, foundWithClusters = 0, foundWithWrongClusters = 0;
            int testWaitTime = (int)(((double) (count + 1) * minSize) / 5512 * 1000);
            var data = GenerateRandomAudioChunks(count);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                                                .BuildFingerprintCommand()
                                                .From(concatenated)
                                                .WithFingerprintConfig(config => 
                                                { 
                                                    config.Clusters = new [] { "USA" };
                                                    return config;
                                                })
                                                .UsingServices(audioService)
                                                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen", concatenated.Duration), hashes);
            
            var cancellationTokenSource = new CancellationTokenSource(testWaitTime);
            var wrong = QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                                              .From(SimulateRealtimeQueryData(data, false))
                                              .WithRealtimeQueryConfig(config =>
                                              {
                                                    config.ResultEntryFilter = new QueryMatchLengthFilter(15d);
                                                    config.SuccessCallback = entry => Interlocked.Increment(ref foundWithWrongClusters);
                                                    config.Clusters = new[] {"CANADA"};
                                                    return config;
                                              })
                                              .UsingServices(modelService)
                                              .Query(cancellationTokenSource.Token);
            
            var right = QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                                .From(SimulateRealtimeQueryData(data, false))
                                .WithRealtimeQueryConfig(config =>
                                {
                                    config.ResultEntryFilter = new QueryMatchLengthFilter(15d);
                                    config.SuccessCallback = entry => Interlocked.Increment(ref foundWithClusters);
                                    config.Clusters = new[] {"USA"};
                                    return config;
                                })
                                .UsingServices(modelService)
                                .Query(cancellationTokenSource.Token);

            await Task.WhenAll(wrong, right);

            Assert.AreEqual(1, foundWithClusters);
            Assert.AreEqual(0, foundWithWrongClusters);
        }
        
        [Test]
        public async Task RealtimeQueryStrideShouldBeUsed()
        {
            var audioService = new SoundFingerprintingAudioService();
            var modelService = new InMemoryModelService();
            int minSize = 8192 + 2048;
            int staticStride = 1024;
            double permittedGap = (double) minSize / 5512;
            int count = 10, found = 0, didNotPassThreshold = 0, fingerprintsCount = 0;
            int testWaitTime = (int)(((double) (count + 1) * minSize) / 5512 * 1000);
            var data = GenerateRandomAudioChunks(count);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                                                .BuildFingerprintCommand()
                                                .From(concatenated)
                                                .UsingServices(audioService)
                                                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen", concatenated.Duration), hashes);
            
            var collection = SimulateRealtimeQueryData(data, false);
            var cancellationTokenSource = new CancellationTokenSource(testWaitTime);
            
            double duration = await QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                                              .From(collection)
                                              .WithRealtimeQueryConfig(config =>
                                              {
                                                    config.Stride = new IncrementalStaticStride(staticStride);
                                                    config.QueryFingerprintsCallback = fingerprints => Interlocked.Add(ref fingerprintsCount, fingerprints.Count);
                                                    config.SuccessCallback = entry => Interlocked.Increment(ref found);
                                                    config.DidNotPassFilterCallback = entry => Interlocked.Increment(ref didNotPassThreshold);
                                                    config.PermittedGap = permittedGap;
                                                    return config;
                                              })
                                              .UsingServices(modelService)
                                              .Query(cancellationTokenSource.Token);

            Assert.AreEqual((count - 1) * minSize / staticStride + 1, fingerprintsCount);
            Assert.AreEqual((double)count * minSize / 5512, duration, 0.00001);
        }
        
        [Test]
        public async Task ShouldQueryInRealtime()
        {
            var audioService = new SoundFingerprintingAudioService();
            var modelService = new InMemoryModelService();

            int count = 10, found = 0, didNotPassThreshold = 0, thresholdVotes = 4, testWaitTime = 30000, fingerprintsCount = 0;
            var data = GenerateRandomAudioChunks(count);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                                                .BuildFingerprintCommand()
                                                .From(concatenated)
                                                .UsingServices(audioService)
                                                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen", concatenated.Duration), hashes);
            
            var collection = SimulateRealtimeQueryData(data, true);

            var realtimeConfig = new RealtimeQueryConfiguration(thresholdVotes, new QueryMatchLengthFilter(10), 
                entry =>
                {
                    Console.WriteLine($"Found Match Starts At {entry.TrackMatchStartsAt:0.000}, Match Length {entry.QueryMatchLength:0.000}, Query Length {entry.QueryLength:0.000} Track Starts At {entry.TrackStartsAt:0.000}");
                    Interlocked.Increment(ref found);
                },
                entry =>
                {
                    Console.WriteLine($"Entry didn't pass filter, Starts At {entry.TrackMatchStartsAt:0.000}, Match Length {entry.QueryMatchLength:0.000}, Query Length {entry.QueryMatchLength:0.000}");
                    Interlocked.Increment(ref didNotPassThreshold);
                },
                fingerprints =>
                {
                    Interlocked.Add(ref fingerprintsCount, fingerprints.Count);
                },
                new IncrementalRandomStride(256, 512), 
                1.48d,
                Enumerable.Empty<string>());

             var cancellationTokenSource = new CancellationTokenSource(testWaitTime);
            
             await QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                                            .From(collection)
                                            .WithRealtimeQueryConfig(realtimeConfig)
                                            .UsingServices(modelService)
                                            .Query(cancellationTokenSource.Token);

            Assert.AreEqual(1, found);
            Assert.AreEqual(1, didNotPassThreshold);
        }

        private static AudioSamples Concatenate(IReadOnlyList<AudioSamples> data)
        {
            int length = 0;
            foreach (var samples in data)
            {
                length += samples.Samples.Length;
            }

            float[] concatenated = new float[length];
            int dest = 0;
            for (int i = 0; i < data.Count; i++)
            {
                Array.Copy(data[i].Samples, 0, concatenated, dest, data[i].Samples.Length);
                dest += data[i].Samples.Length;
            }
            
            return new AudioSamples(concatenated, "Queen", 5512);
        }

        private static List<AudioSamples> GenerateRandomAudioChunks(int count)
        {
            var list = new List<AudioSamples>();
            for (int i = 0; i < count; ++i)
            {
                var audioSamples = GetMinSizeOfAudioSamples();
                list.Add(audioSamples);
            }

            return list;
        }

        private static BlockingCollection<AudioSamples> SimulateRealtimeQueryData(IReadOnlyCollection<AudioSamples> audioSamples, bool jitter)
        {
            var collection = new BlockingCollection<AudioSamples>();
            Task.Factory.StartNew(async () =>
            {
                if (jitter)
                {
                    await Jitter(collection);
                }

                foreach (var audioSample in audioSamples)
                {
                    collection.Add(audioSample);
                    await Task.Delay(TimeSpan.FromSeconds(audioSample.Duration));
                }

                if (jitter)
                {
                    await Jitter(collection);
                }

                collection.CompleteAdding();
            });

            return collection;
        }

        private static async Task Jitter(BlockingCollection<AudioSamples> collection)
        {
            for (int i = 0; i < 5; ++i)
            {
                var audioSample = GetMinSizeOfAudioSamples();
                collection.Add(audioSample);
                await Task.Delay(TimeSpan.FromSeconds(audioSample.Duration));
            }
        }

        private static AudioSamples GetMinSizeOfAudioSamples()
        {
            var samples = TestUtilities.GenerateRandomFloatArray(10240);
            return new AudioSamples(samples, "cnn", 5512);
        }
    }
}