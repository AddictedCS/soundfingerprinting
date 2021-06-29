namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices.ComTypes;
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
            var data = GenerateRandomAudioChunks(count, 1, DateTime.UtcNow);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                                                .BuildFingerprintCommand()
                                                .From(concatenated)
                                                .UsingServices(audioService)
                                                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen", new Dictionary<string, string>{{ "country", "USA" }}), hashes);
            
            var cancellationTokenSource = new CancellationTokenSource(testWaitTime);
            var wrong = QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                                              .From(SimulateRealtimeQueryData(data, jitterLength: 0))
                                              .WithRealtimeQueryConfig(config =>
                                              {
                                                    config.ResultEntryFilter = new TrackMatchLengthEntryFilter(15d);
                                                    config.SuccessCallback = _ => Interlocked.Increment(ref foundWithWrongClusters);
                                                    config.YesMetaFieldsFilter = new Dictionary<string, string> {{"country", "CANADA"}};
                                                    return config;
                                              })
                                              .UsingServices(modelService)
                                              .Query(cancellationTokenSource.Token);
            
            var right = QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                                .From(SimulateRealtimeQueryData(data, jitterLength: 0))
                                .WithRealtimeQueryConfig(config =>
                                {
                                    config.ResultEntryFilter = new TrackMatchLengthEntryFilter(15d);
                                    config.SuccessCallback = _ => Interlocked.Increment(ref foundWithClusters);
                                    config.YesMetaFieldsFilter = new Dictionary<string, string> {{"country", "USA"}};
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
            var data = GenerateRandomAudioChunks(count, 1, DateTime.UtcNow);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                                                .BuildFingerprintCommand()
                                                .From(concatenated)
                                                .UsingServices(audioService)
                                                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen"), hashes);
            
            var collection = SimulateRealtimeQueryData(data, jitterLength: 0);
            var cancellationTokenSource = new CancellationTokenSource(testWaitTime);
            
            double duration = await QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                                              .From(collection)
                                              .WithRealtimeQueryConfig(config =>
                                              {
                                                    config.Stride = new IncrementalStaticStride(staticStride);
                                                    config.SuccessCallback = _ => Interlocked.Increment(ref found);
                                                    config.DidNotPassFilterCallback = _ => Interlocked.Increment(ref didNotPassThreshold);
                                                    config.PermittedGap = permittedGap;
                                                    return config;
                                              })
                                              .Intercept(fingerprints =>
                                              {
                                                  Interlocked.Add(ref fingerprintsCount, fingerprints.Count);
                                                  return fingerprints;
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

            const double minSizeChunk = 10240d / 5512; // length in seconds of one query chunk ~1.8577
            const double totalTrackLength = 210;       // length of the track 3 minutes 30 seconds.
            int count = (int)(totalTrackLength / minSizeChunk), fingerprintsCount = 0, queryMatchLength = 10, ongoingCalls = 0;
            var data = GenerateRandomAudioChunks(count, seed: 1, DateTime.UtcNow);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                                                .BuildFingerprintCommand()
                                                .From(concatenated)
                                                .UsingServices(audioService)
                                                .Hash();

            // hashes have to be equal to total track length +- 1 second
            Assert.AreEqual(totalTrackLength, hashes.DurationInSeconds, delta: 1);
            
            // store track data and associated hashes
            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen"), hashes);

            var successMatches = new List<ResultEntry>();
            var didNotGetToContiguousQueryMatchLengthMatch = new List<ResultEntry>();
            
            var realtimeConfig = new RealtimeQueryConfiguration(thresholdVotes: 4, new TrackMatchLengthEntryFilter(queryMatchLength), 
                successCallback: entry =>
                {
                    Console.WriteLine($"Found Match Starts At {entry.TrackMatchStartsAt:0.000}, Match Length {entry.TrackCoverageWithPermittedGapsLength:0.000}, Query Length {entry.QueryLength:0.000} Track Starts At {entry.TrackStartsAt:0.000}");
                    successMatches.Add(entry);
                },
                didNotPassFilterCallback: entry =>
                {
                    Console.WriteLine($"Entry didn't pass filter, Starts At {entry.TrackMatchStartsAt:0.000}, Match Length {entry.TrackCoverageWithPermittedGapsLength:0.000}, Query Length {entry.TrackCoverageWithPermittedGapsLength:0.000}");
                    didNotGetToContiguousQueryMatchLengthMatch.Add(entry);
                },
                new OngoingRealtimeResultEntryFilter(minCoverage: 0.2d, minTrackLength: 1d),
                ongoingSuccessCallback: _ => { Interlocked.Increment(ref ongoingCalls); },
                errorCallback: (error, _) => throw error,
                restoredAfterErrorCallback: () => throw new Exception("Downtime callback called"),
                downtimeHashes: Enumerable.Empty<Hashes>(), 
                stride: new IncrementalRandomStride(256, 512), 
                permittedGap: 2d,
                downtimeCapturePeriod: 0d,
                yesMetaFieldFilters: new Dictionary<string, string>(),
                noMetaFieldsFilters: new Dictionary<string, string>());

            // simulating realtime query, starting in the middle of the track ~1 min 45 seconds (105 seconds).
            // and we query for 35 seconds
            const double queryLength = 35;
            // track  ---------------------------- 210 seconds
            // query                ----           35 seconds
            // match starts at      |              105 second
            var realtimeQuery = data.Skip(count / 2).Take((int)(queryLength/minSizeChunk) + 1).ToArray(); 
            
            Assert.AreEqual(queryLength, realtimeQuery.Sum(_ => _.Duration), 1); // asserting the total length of the query +- 1 second
            
            // adding some jitter before and after the query which should not match
            // track           ---------------------------- 210 seconds
            // q with jitter             ^^^----^^^         10 sec + 35 seconds + 10 sec = 55 sec
            // match starts at              |               105 second
            const double jitterLength = 10;
            var collection = SimulateRealtimeQueryData(realtimeQuery, jitterLength);
            double processed = await QueryCommandBuilder.Instance
                                            .BuildRealtimeQueryCommand()
                                            .From(collection)
                                            .WithRealtimeQueryConfig(realtimeConfig)
                                            .Intercept(fingerprints =>
                                            {
                                                Interlocked.Add(ref fingerprintsCount, fingerprints.Count);
                                                return fingerprints;
                                            })
                                            .UsingServices(modelService)
                                            .Query(CancellationToken.None);

            // since we start from the middle of the track ~1 min 45 seconds and query for 35 seconds with 10 seconds filter
            // this means we will get 3 successful matches 
            // start: 105 seconds,  query length: 35 seconds, query match filter length: 10
            int matchesCount = (int)Math.Floor(queryLength / queryMatchLength);
            Assert.AreEqual(matchesCount, successMatches.Count);
            
            // since our realtime query was 35 seconds with 3 successful matches of 10
            // there has to be one more purged match of 5 seconds which did not get through successful filter
            Assert.AreEqual(1, didNotGetToContiguousQueryMatchLengthMatch.Count);
            
            // verifying that we queried the correct amount of seconds
            Assert.AreEqual(queryLength + 2 * jitterLength, processed, 1);

            // track starts to match at the middle
            // matches                      |||
            // q with jitter             ^^^---^^^        
            // expecting 3 matches at 105th, 115th and 125th second 
            double[] trackMatches = Enumerable.Repeat(totalTrackLength / 2, matchesCount).Select((matchAt, index) => matchAt + queryMatchLength * index).ToArray();
            for (int i = 0; i < trackMatches.Length; ++i)
            {
                Assert.AreEqual(trackMatches[i], successMatches[i].TrackMatchStartsAt, 2.5);
            }
            
            // ongoing calls have to be called every time when realtime chunk is sent since ongoing query match filter is equal to min-size chunk
            Assert.AreEqual(realtimeQuery.Length, ongoingCalls);
        }

        [Test]
        public async Task ShouldNotLoseAudioSamplesInCaseIfExceptionIsThrown()
        {
            var audioService = new SoundFingerprintingAudioService();
            var modelService = new InMemoryModelService();

            const double minSizeChunk = 10240d / 5512; // length in seconds of one query chunk ~1.8577
            const double totalTrackLength = 210;       // length of the track 3 minutes 30 seconds.
            const double jitterLength = 10;
            double totalQueryLength = totalTrackLength + 2 * jitterLength;
            
            int trackCount = (int)(totalTrackLength / minSizeChunk), fingerprintsCount = 0, errored = 0, didNotPassThreshold = 0, jitterChunks = 2;
            
            var start = new DateTime(2021, 5, 1, 0, 0, 0, DateTimeKind.Utc);
            var data = GenerateRandomAudioChunks(trackCount, seed: 1, start);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                                                .BuildFingerprintCommand()
                                                .From(concatenated)
                                                .UsingServices(audioService)
                                                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen"), hashes);

            var resultEntries = new List<ResultEntry>();
            var collection = SimulateRealtimeQueryData(data, jitterLength);

            var offlineStorage = new OfflineStorage(Path.GetTempPath());
            var restoreCalled = new bool[1];
            double processed = await new RealtimeQueryCommand(FingerprintCommandBuilder.Instance, new FaultyQueryService(faultyCounts: trackCount + jitterChunks - 1, QueryFingerprintService.Instance))
                 .From(collection)
                 .WithRealtimeQueryConfig(config =>
                 {
                     config.SuccessCallback = entry =>
                     {
                         resultEntries.Add(entry);
                     };

                     config.DidNotPassFilterCallback = _ => Interlocked.Increment(ref didNotPassThreshold);
                     config.ErrorCallback = (_, timedHashes) =>
                     {
                         Interlocked.Increment(ref errored);
                         offlineStorage.Save(timedHashes);
                     };

                     config.ResultEntryFilter = new TrackRelativeCoverageLengthEntryFilter(0.4, waitTillCompletion: true);
                     config.RestoredAfterErrorCallback = () => restoreCalled[0] = true;
                     config.DowntimeHashes = offlineStorage;              // store the other half of the fingerprints in the downtime hashes storage
                     config.DowntimeCapturePeriod = totalQueryLength / 2; // store half of the fingerprints in the offline storage
                     return config;
                 })
                 .Intercept(fingerprints =>
                 {
                     Interlocked.Increment(ref fingerprintsCount);
                     return fingerprints;
                 })
                 .UsingServices(modelService)
                 .Query(CancellationToken.None);

            Assert.AreEqual(totalQueryLength, processed, 1);
            Assert.AreEqual(trackCount + jitterChunks - 1, errored);
            Assert.AreEqual(trackCount, fingerprintsCount - jitterChunks);
            Assert.IsTrue(restoreCalled[0]);
            Assert.AreEqual(0, didNotPassThreshold);
            Assert.AreEqual(1, resultEntries.Count);
            var result = resultEntries.First();
            Assert.AreEqual(totalTrackLength, result.Coverage.TrackCoverageWithPermittedGapsLength, 1);
            Assert.IsTrue(Math.Abs(start.Subtract(result.MatchedAt).TotalSeconds) < 2, $"Matched At {result.MatchedAt:o}");
        }

        [Test]
        public async Task HashesShouldMatchExactlyWhenAggregated()
        {
            var audioService = new SoundFingerprintingAudioService();
            var modelService = new InMemoryModelService();

            int count = 20;
            var data = GenerateRandomAudioChunks(count, seed: 1, DateTime.UtcNow);
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
            
            var collection = SimulateRealtimeQueryData(data, jitterLength: 0);
            var list = new List<Hashes>();
            
            await QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                .From(collection)
                .WithRealtimeQueryConfig(config =>
                {
                    config.Stride = new IncrementalStaticStride(512);
                    return config;
                })
                .Intercept(timedHashes =>
                {
                    list.Add(timedHashes);
                    return timedHashes;
                })
                .UsingServices(modelService)
                .Query(CancellationToken.None);
            
            Assert.AreEqual(hashes.Count, list.Select(entry => entry.Count).Sum());
            var merged = Hashes.Aggregate(list, concatenated.Duration).ToList();
            Assert.AreEqual(1, merged.Count, $"Hashes:{string.Join(",", merged.Select(_ => $"{_.RelativeTo},{_.DurationInSeconds:0.00}"))}");
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
            var data = GenerateRandomAudioChunks(count, 1, DateTime.UtcNow);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(concatenated)
                .WithFingerprintConfig(config => config)
                .UsingServices(audioService)
                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen"), hashes);

            var collection = SimulateRealtimeQueryData(data, jitterLength: 0);
            var cancellationTokenSource = new CancellationTokenSource(testWaitTime);
            var fingerprints = new List<Hashes>();
            var entries = new List<ResultEntry>();
            
            await QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                .From(collection)
                .WithRealtimeQueryConfig(config =>
                {
                    config.SuccessCallback = entry => entries.Add(entry);
                    config.ResultEntryFilter = new TrackRelativeCoverageLengthEntryFilter(0.8d);
                    config.Stride = new IncrementalStaticStride(2048);
                    return config;
                })
                .Intercept(queryHashes =>
                {
                    fingerprints.Add(queryHashes);
                    return queryHashes;
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

        [Test]
        public async Task ShouldPurgeCompletedMatchWhenAsyncCollectionIsExhausted()
        {
            var modelService = new InMemoryModelService();

            const double minSizeChunk = 10240d / 5512; // length in seconds of one query chunk ~1.8577
            const double totalTrackLength = 210;       // length of the track 3 minutes 30 seconds.

            var data = GenerateRandomAudioChunks((int)(totalTrackLength / minSizeChunk), seed: 1, DateTime.UtcNow);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(concatenated)
                .UsingServices(new SoundFingerprintingAudioService())
                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen"), hashes);

            var list = new List<ResultEntry>();
            await QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                .From(SimulateRealtimeQueryData(data, jitterLength: 0))
                .WithRealtimeQueryConfig(config =>
                {
                    config.ResultEntryFilter = new TrackRelativeCoverageLengthEntryFilter(0.5, true);
                    config.SuccessCallback = entry => { list.Add(entry); };
                    return config;
                })
                .UsingServices(modelService)
                .Query(CancellationToken.None);

            Assert.AreEqual(1, list.Count);
        }

        private static AudioSamples Concatenate(IReadOnlyList<AudioSamples> data)
        {
            int length = data.Sum(samples => samples.Samples.Length);
            float[] concatenated = new float[length];
            int dest = 0;
            foreach (var audioSamples in data)
            {
                Array.Copy(audioSamples.Samples, 0, concatenated, dest, audioSamples.Samples.Length);
                dest += audioSamples.Samples.Length;
            }

            var first = data.First();
            return new AudioSamples(concatenated, first.Origin, first.SampleRate, first.RelativeTo);
        }

        private static List<AudioSamples> GenerateRandomAudioChunks(int count, int seed, DateTime relativeTo)
        {
            return Enumerable
                .Range(0, count)
                .Select(index => GetMinSizeOfAudioSamples(seed * index, relativeTo.AddSeconds(index * 1.877)))
                .ToList();
        }

        /// <summary>
        ///  Simulate realtime query data.
        /// </summary>
        /// <param name="audioSamples">Chunks of audio samples.</param>
        /// <param name="jitterLength">Jitter length in seconds, added at the beginning and at the end.</param>
        /// <returns>Async enumerable.</returns>
        private static IAsyncEnumerable<AudioSamples> SimulateRealtimeQueryData(IReadOnlyCollection<AudioSamples> audioSamples, double jitterLength)
        {
            var collection = new BlockingCollection<AudioSamples>();
            Task.Factory.StartNew(() =>
            {
                if (jitterLength > 0)
                {
                    var startAt = audioSamples.First()?.RelativeTo.AddSeconds(-jitterLength) ?? DateTime.UtcNow;
                    Jitter(collection, jitterLength, startAt);
                }

                foreach (var audioSample in audioSamples)
                {
                    collection.Add(new AudioSamples(audioSample.Samples, audioSample.Origin, audioSample.SampleRate, audioSample.RelativeTo));
                }

                if (jitterLength > 0)
                {
                    var endsAt = audioSamples.Last()?.RelativeTo ?? DateTime.UtcNow;
                    Jitter(collection, jitterLength, endsAt);
                }

                collection.CompleteAdding();
            });

            return new BlockingRealtimeCollection<AudioSamples>(collection);
        }

        private static void Jitter(BlockingCollection<AudioSamples> collection, double jitterLength, DateTime dateTime)
        {
            double sum = 0d;
            do
            {
                var audioSample = TestUtilities.GenerateRandomAudioSamples((int)(jitterLength * 5512));
                collection.Add(new AudioSamples(audioSample.Samples, audioSample.Origin, audioSample.SampleRate, dateTime));
                sum += audioSample.Duration;
            } 
            while (sum < jitterLength);
        }

        private static AudioSamples GetMinSizeOfAudioSamples(int seed, DateTime relativeTo)
        {
            var samples = TestUtilities.GenerateRandomFloatArray(10240, seed);
            return new AudioSamples(samples, "cnn", 5512, relativeTo);
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