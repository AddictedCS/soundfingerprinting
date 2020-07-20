namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
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
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Strides;

    [TestFixture]
    public class RealtimeQueryCommandTest
    {
        [Test]
        public async Task RealtimeQueryShouldMatchOnlySelectedClusters()
        {
            var audioService = new SoundFingerprintingAudioService();
            var modelService = new InMemoryModelService();
            int count = 10, foundWithClusters = 0, foundWithWrongClusters = 0, testWaitTime = 3000;
            var data = GenerateRandomAudioChunks(count, 1);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                                                .BuildFingerprintCommand()
                                                .From(concatenated)
                                                .UsingServices(audioService)
                                                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen", new Dictionary<string, string>{{ "country", "USA" }}), hashes);
            
            var cancellationTokenSource = new CancellationTokenSource(testWaitTime);
            var wrong = QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                                              .From(SimulateRealtimeQueryData(data, false, TimeSpan.FromMilliseconds))
                                              .WithRealtimeQueryConfig(config =>
                                              {
                                                    config.ResultEntryFilter = new QueryMatchLengthFilter(15d);
                                                    config.SuccessCallback = entry => Interlocked.Increment(ref foundWithWrongClusters);
                                                    config.MetaFieldsFilter = new Dictionary<string, string> {{"country", "CANADA"}};
                                                    return config;
                                              })
                                              .UsingServices(modelService)
                                              .Query(cancellationTokenSource.Token);
            
            var right = QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                                .From(SimulateRealtimeQueryData(data, false, TimeSpan.FromMilliseconds))
                                .WithRealtimeQueryConfig(config =>
                                {
                                    config.ResultEntryFilter = new QueryMatchLengthFilter(15d);
                                    config.SuccessCallback = entry => Interlocked.Increment(ref foundWithClusters);
                                    config.MetaFieldsFilter = new Dictionary<string, string> {{"country", "USA"}};
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
            int testWaitTime = 3000;
            var data = GenerateRandomAudioChunks(count, 1);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                                                .BuildFingerprintCommand()
                                                .From(concatenated)
                                                .UsingServices(audioService)
                                                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen"), hashes);
            
            var collection = SimulateRealtimeQueryData(data, false, TimeSpan.FromMilliseconds);
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

            int count = 10, found = 0, didNotPassThreshold = 0, thresholdVotes = 4, testWaitTime = 5000, fingerprintsCount = 0;
            var data = GenerateRandomAudioChunks(count, 1);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                                                .BuildFingerprintCommand()
                                                .From(concatenated)
                                                .UsingServices(audioService)
                                                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen"), hashes);
            
            var collection = SimulateRealtimeQueryData(data, true, TimeSpan.FromMilliseconds);

            var realtimeConfig = new RealtimeQueryConfiguration(thresholdVotes, new QueryMatchLengthFilter(10), 
                entry =>
                {
                    Console.WriteLine($"Found Match Starts At {entry.TrackMatchStartsAt:0.000}, Match Length {entry.CoverageWithPermittedGapsLength:0.000}, Query Length {entry.QueryLength:0.000} Track Starts At {entry.TrackStartsAt:0.000}");
                    Interlocked.Increment(ref found);
                },
                entry =>
                {
                    Console.WriteLine($"Entry didn't pass filter, Starts At {entry.TrackMatchStartsAt:0.000}, Match Length {entry.CoverageWithPermittedGapsLength:0.000}, Query Length {entry.CoverageWithPermittedGapsLength:0.000}");
                    Interlocked.Increment(ref didNotPassThreshold);
                },
                fingerprints => Interlocked.Add(ref fingerprintsCount, fingerprints.Count),
                (error, _) => throw error,
                () => throw new Exception("Downtime callback called"),
                Enumerable.Empty<Hashes>(), 
                new IncrementalRandomStride(256, 512), 
                1.48d,
                0d,
                (int)(10240d/5512) * 1000,
                new Dictionary<string, string>());

             var cancellationTokenSource = new CancellationTokenSource(testWaitTime);
            
            double processed = await QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                                            .From(collection)
                                            .WithRealtimeQueryConfig(realtimeConfig)
                                            .UsingServices(modelService)
                                            .Query(cancellationTokenSource.Token);

            Assert.AreEqual(1, found);
            Assert.AreEqual(1, didNotPassThreshold);
            Assert.AreEqual((count + 10) * 10240 / 5512d, processed, 0.2);
        }

        [Test]
        public async Task ShouldNotLoseAudioSamplesInCaseIfExceptionIsThrown()
        {
            var audioService = new SoundFingerprintingAudioService();
            var modelService = new InMemoryModelService();

            int count = 10, found = 0, didNotPassThreshold = 0, thresholdVotes = 4, testWaitTime = 40000, fingerprintsCount = 0, errored = 0;
            var data = GenerateRandomAudioChunks(count, 1);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                                                .BuildFingerprintCommand()
                                                .From(concatenated)
                                                .UsingServices(audioService)
                                                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen"), hashes);

            var started = DateTime.Now;
            var resultEntries = new List<ResultEntry>();
            var collection = SimulateRealtimeQueryData(data, true, TimeSpan.FromSeconds);

            var cancellationTokenSource = new CancellationTokenSource(testWaitTime);
            var offlineStorage = new OfflineStorage(Path.GetTempPath());
            var restoreCalled = new bool[1];
            double processed = await new RealtimeQueryCommand(FingerprintCommandBuilder.Instance, new FaultyQueryService(count, QueryFingerprintService.Instance))
                 .From(collection)
                 .WithRealtimeQueryConfig(config =>
                 {
                     config.SuccessCallback = entry =>
                     {
                         Interlocked.Increment(ref found);
                         resultEntries.Add(entry);
                     };

                     config.QueryFingerprintsCallback = fingerprints => Interlocked.Increment(ref fingerprintsCount);
                     config.DidNotPassFilterCallback = entry => Interlocked.Increment(ref didNotPassThreshold);
                     config.ErrorCallback = (exception, timedHashes) =>
                     {
                         Interlocked.Increment(ref errored);
                         offlineStorage.Save(timedHashes);
                     };
                     
                     config.ResultEntryFilter = new QueryMatchLengthFilter(10);
                     config.RestoredAfterErrorCallback = () => restoreCalled[0] = true;
                     config.PermittedGap = 1.48d;
                     config.ThresholdVotes = thresholdVotes;
                     config.DowntimeHashes = offlineStorage;
                     config.DowntimeCapturePeriod = 3d;
                     return config;
                 })
                 .UsingServices(modelService)
                 .Query(cancellationTokenSource.Token);

            Assert.AreEqual(count, errored);
            Assert.AreEqual(20, fingerprintsCount);
            Assert.IsTrue(restoreCalled[0]);
            Assert.AreEqual(1, found);
            var resultEntry = resultEntries[0];
            double jitterLength = 5 * 10240 / 5512d;
            Assert.AreEqual(0d, started.AddSeconds(jitterLength + resultEntry.TrackMatchStartsAt).Subtract(resultEntry.MatchedAt).TotalSeconds, 2d);
            Assert.AreEqual(1, didNotPassThreshold);
            Assert.AreEqual((count + 10) * 10240 / 5512d, processed, 0.2);
        }

        [Test]
        public async Task HashesShouldMatchExactlyWhenAggregated()
        {
            var audioService = new SoundFingerprintingAudioService();
            var modelService = new InMemoryModelService();

            int count = 20, testWaitTime = 40000;
            var data = GenerateRandomAudioChunks(count, 1);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(concatenated)
                .WithFingerprintConfig(config =>
                {
                    config.Stride = new IncrementalStaticStride(512);
                    return config;
                })
                .UsingServices(audioService)
                .Hash();
            
            var collection = SimulateRealtimeQueryData(data, false, TimeSpan.FromSeconds);
            var cancellationTokenSource = new CancellationTokenSource(testWaitTime);
            var list = new List<Hashes>();
            
            await QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                .From(collection)
                .WithRealtimeQueryConfig(config =>
                {
                    config.QueryFingerprintsCallback += timedHashes => list.Add(timedHashes);
                    config.Stride = new IncrementalStaticStride(512);
                    return config;
                })
                .UsingServices(modelService)
                .Query(cancellationTokenSource.Token);
            
            Assert.AreEqual(hashes.Count, list.Select(entry => entry.Count).Sum());
            var merged = Hashes.Aggregate(list, 20d).ToList();
            Assert.AreEqual(2, merged.Count, $"Hashes:{string.Join(",", merged.Select(_ => $"{_.RelativeTo},{_.DurationInSeconds:0.00}"))}");
            Assert.AreEqual(hashes.Count, merged.Select(entry => entry.Count).Sum());

            var aggregated = Hashes.Aggregate(list, double.MaxValue).ToList();
            Assert.AreEqual(1, aggregated.Count);
            Assert.AreEqual(hashes.Count, aggregated[0].Count);
            foreach (var zipped in hashes.OrderBy(h => h.SequenceNumber).Zip(aggregated[0], (a, b) => new { a, b }))
            {
                Assert.AreEqual(zipped.a.StartsAt, zipped.b.StartsAt, 1d);
                Assert.AreEqual(zipped.a.SequenceNumber, zipped.b.SequenceNumber);
                CollectionAssert.AreEqual(zipped.a.HashBins, zipped.b.HashBins);
            }
        }

        [Test]
        public async Task QueryingWithAggregatedHashesShouldResultInTheSameMatches()
        {
            var audioService = new SoundFingerprintingAudioService();
            var modelService = new InMemoryModelService();

            int count = 20, testWaitTime = 5000;
            var data = GenerateRandomAudioChunks(count, 1);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(concatenated)
                .WithFingerprintConfig(config => config)
                .UsingServices(audioService)
                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen"), hashes);

            var collection = SimulateRealtimeQueryData(data, false, TimeSpan.FromMilliseconds);
            var cancellationTokenSource = new CancellationTokenSource(testWaitTime);
            var fingerprints = new List<Hashes>();
            var entries = new List<ResultEntry>();
            
            await QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                .From(collection)
                .WithRealtimeQueryConfig(config =>
                {
                    config.QueryFingerprintsCallback += queryHashes => fingerprints.Add(queryHashes);
                    config.SuccessCallback = entry => entries.Add(entry);
                    config.ResultEntryFilter = new CoverageLengthEntryFilter(0.8d);
                    config.Stride = new IncrementalStaticStride(2048);
                    return config;
                })
                .UsingServices(modelService)
                .Query(cancellationTokenSource.Token);

            Assert.IsTrue(entries.Any());
            Assert.AreEqual(1, entries.Count);
            var realtimeResult = entries.First();
            var aggregatedHashes = Hashes.Aggregate(fingerprints, 60d).First();
            var nonRealtimeResult = await QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(aggregatedHashes)
                .UsingServices(modelService, audioService)
                .Query();
            
            Assert.IsTrue(nonRealtimeResult.ContainsMatches);
            Assert.AreEqual(1, nonRealtimeResult.ResultEntries.Count());
            Assert.AreEqual(realtimeResult.MatchedAt, aggregatedHashes.RelativeTo);
            Assert.AreEqual(realtimeResult.MatchedAt, nonRealtimeResult.BestMatch.MatchedAt, $"Realtime vs NonRealtime {nonRealtimeResult.BestMatch.Coverage.BestPath.Count()} match time does not match");
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
            foreach (var audioSamples in data)
            {
                Array.Copy(audioSamples.Samples, 0, concatenated, dest, audioSamples.Samples.Length);
                dest += audioSamples.Samples.Length;
            }
            
            return new AudioSamples(concatenated, "Queen", 5512);
        }

        private static List<AudioSamples> GenerateRandomAudioChunks(int count, int seed)
        {
            var list = new List<AudioSamples>();
            for (int i = 0; i < count; ++i)
            {
                var audioSamples = GetMinSizeOfAudioSamples(seed * i);
                list.Add(audioSamples);
            }

            return list;
        }

        private static BlockingCollection<AudioSamples> SimulateRealtimeQueryData(IReadOnlyCollection<AudioSamples> audioSamples, bool jitter, Func<double, TimeSpan> waitTime)
        {
            var collection = new BlockingCollection<AudioSamples>();
            Task.Factory.StartNew(async () =>
            {
                if (jitter)
                {
                    await Jitter(collection, waitTime);
                }

                foreach (var audioSample in audioSamples)
                {
                    await Task.Delay(waitTime(audioSample.Duration));
                    collection.Add(new AudioSamples(audioSample.Samples, audioSample.Origin, audioSample.SampleRate));
                }

                if (jitter)
                {
                    await Jitter(collection, waitTime);
                }

                collection.CompleteAdding();
            });

            return collection;
        }

        private static async Task Jitter(BlockingCollection<AudioSamples> collection, Func<double, TimeSpan> waitTime)
        {
            for (int i = 0; i < 5; ++i)
            {
                var audioSample = GetMinSizeOfAudioSamples(0);
                await Task.Delay(waitTime(audioSample.Duration));
                collection.Add(audioSample);
            }
        }

        private static AudioSamples GetMinSizeOfAudioSamples(int seed)
        {
            var samples = TestUtilities.GenerateRandomFloatArray(10240, seed);
            return new AudioSamples(samples, "cnn", 5512);
        }

        private class FaultyQueryService : IQueryFingerprintService
        {
            private readonly IQueryFingerprintService goodOne;

            private int faultyCounts;

            public FaultyQueryService(int faultyCounts, IQueryFingerprintService goodOne)
            {
                this.faultyCounts = faultyCounts;
                this.goodOne = goodOne;
            }
            
            public QueryResult Query(Hashes queryFingerprints, QueryConfiguration configuration, IModelService modelService)
            {
                if (faultyCounts-- > 0)
                {
                    throw new IOException("I/O exception");
                }

                return goodOne.Query(queryFingerprints, configuration, modelService);
            }
        }
    }
}