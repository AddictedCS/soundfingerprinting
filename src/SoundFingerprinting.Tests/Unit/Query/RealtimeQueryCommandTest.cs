namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NLog.Extensions.Logging;
    using NUnit.Framework;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Content;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Media;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Strides;

    [TestFixture]
    public class RealtimeQueryCommandTest
    {
        private readonly ILogger<RealtimeQueryCommandTest> logger = new NLogLoggerFactory().CreateLogger<RealtimeQueryCommandTest>();

        private const int SampleRate = 5512;
        private readonly int minSamplesPerFingerprint;
        private readonly double minSizeChunkDuration;
        
        public RealtimeQueryCommandTest()
        {
            var config = new DefaultFingerprintConfiguration();
            minSamplesPerFingerprint = config.SpectrogramConfig.MinimumSamplesPerFingerprint;
            minSizeChunkDuration = (double)minSamplesPerFingerprint / SampleRate;
        }
        
        [Test]
        public async Task RealtimeQueryShouldMatchOnlySelectedMetaFieldsFilters()
        {
            var modelService = new InMemoryModelService();
            int count = 10, foundWithClusters = 0, foundWithWrongClusters = 0, testWaitTime = 3000;
            var data = GenerateRandomAudioChunks(count, 1, DateTime.UtcNow);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                                                .BuildFingerprintCommand()
                                                .From(concatenated)
                                                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen", new Dictionary<string, string>{{ "country", "USA" }}), hashes);
            
            var cancellationTokenSource = new CancellationTokenSource(testWaitTime);
            var wrong = QueryCommandBuilder
                                              .Instance
                                              .BuildRealtimeQueryCommand()
                                              .From(SimulateRealtimeAudioQueryData(data, jitterLength: 0))
                                              .WithRealtimeQueryConfig(config =>
                                              {
                                                    config.ResultEntryFilter = new TrackCoverageLengthEntryFilter(15d, waitTillCompletion: false);
                                                    config.SuccessCallback = _ => Interlocked.Increment(ref foundWithWrongClusters);
                                                    config.YesMetaFieldsFilter = new Dictionary<string, string> {{"country", "CANADA"}};
                                                    return config;
                                              })
                                              .UsingServices(modelService)
                                              .Query(cancellationTokenSource.Token);
            
            var right = QueryCommandBuilder.Instance
                                .BuildRealtimeQueryCommand()
                                .From(SimulateRealtimeAudioQueryData(data, jitterLength: 0))
                                .WithRealtimeQueryConfig(config =>
                                {
                                    config.ResultEntryFilter = new TrackCoverageLengthEntryFilter(15d, waitTillCompletion: false);
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
            var modelService = new InMemoryModelService();
            int staticStride = 1024;
            double permittedGap = (double) minSamplesPerFingerprint / SampleRate;
            int count = 10, found = 0, didNotPassThreshold = 0, fingerprintsCount = 0, testWaitTime = 3000;
            var data = GenerateRandomAudioChunks(count, 1, DateTime.UtcNow);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                                                .BuildFingerprintCommand()
                                                .From(concatenated)
                                                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen"), hashes);
            
            var collection = SimulateRealtimeAudioQueryData(data, jitterLength: 0);
            var cancellationTokenSource = new CancellationTokenSource(testWaitTime);
            
            double duration = await QueryCommandBuilder.Instance
                                              .BuildRealtimeQueryCommand()
                                              .From(collection)
                                              .WithRealtimeQueryConfig(config =>
                                              {
                                                    config.QueryConfiguration.Audio.Stride = new IncrementalStaticStride(staticStride);
                                                    config.SuccessCallback = _ => Interlocked.Increment(ref found);
                                                    config.DidNotPassFilterCallback = _ => Interlocked.Increment(ref didNotPassThreshold);
                                                    config.QueryConfiguration.Audio.PermittedGap = permittedGap;
                                                    return config;
                                              })
                                              .InterceptHashes(fingerprints =>
                                              {
                                                  Interlocked.Add(ref fingerprintsCount, fingerprints.Audio?.Count ?? 0 + fingerprints.Video?.Count ?? 0);
                                                  return fingerprints;
                                              })
                                              .UsingServices(modelService)
                                              .Query(cancellationTokenSource.Token);

            Assert.AreEqual((count - 1) * minSamplesPerFingerprint / staticStride + 1, fingerprintsCount);
            Assert.AreEqual((double)count * minSamplesPerFingerprint / 5512, duration, 0.00001);
        }
        
        [Test]
        public async Task ShouldCaptureRealtimeQueryResultThatOccursOnTheEdgeOfQueryMatches()
        {
            var modelService = new InMemoryModelService();
            int staticStride = 512;
            double permittedGap = (double) minSamplesPerFingerprint / SampleRate;
            int count = 10, found = 0, didNotPassThreshold = 0, hashesCount = 0, testWaitTime = 3000;
            var data = GenerateRandomAudioChunks(count, 1, DateTime.UtcNow);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                                                .BuildFingerprintCommand()
                                                .From(concatenated)
                                                .Hash();
            
            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen"), hashes);

            // query samples contain all but last chunk of the track and more random samples that should not match
            var querySamples = data
                .Take(count - 1)
                .Concat(GenerateRandomAudioChunks(count, seed: 100, relativeTo: DateTime.UtcNow.AddSeconds(minSizeChunkDuration * (count - 1)))).ToList();
            
            var collection = SimulateRealtimeAudioQueryData(querySamples, jitterLength: 0);
            var cancellationTokenSource = new CancellationTokenSource(testWaitTime);
            
            await QueryCommandBuilder.Instance
                .BuildRealtimeQueryCommand()
                .From(collection)
                .WithRealtimeQueryConfig(config =>
                {
                    config.ResultEntryFilter = new TrackRelativeCoverageEntryFilter(0.2d, waitTillCompletion: true);
                    config.QueryConfiguration.Audio.Stride = new IncrementalStaticStride(staticStride);
                    config.SuccessCallback = _ =>
                    {
                        // we want to capture match not when final results are purged
                        // making sure we call the accumulators even on empty results
                        if (hashesCount <= count + 1)
                        {
                            Interlocked.Increment(ref found);
                        }
                    };
                    config.DidNotPassFilterCallback = _ => Interlocked.Increment(ref didNotPassThreshold);
                    config.QueryConfiguration.Audio.PermittedGap = permittedGap;
                    return config;
                })
                .InterceptHashes(fingerprints =>
                {
                    Interlocked.Increment(ref hashesCount);
                    return fingerprints;
                })
                .UsingServices(modelService)
                .Query(cancellationTokenSource.Token); 
            
            Assert.AreEqual(1, found);
            Assert.AreEqual(count * 2 - 1, hashesCount);
        }
        
        [Test]
        public async Task ShouldQueryInRealtime()
        {
            var modelService = new InMemoryModelService();

            double minSizeChunk = (double)minSamplesPerFingerprint / SampleRate; // length in seconds of one query chunk ~1.8577
            const double totalTrackLength = 210;                                 // length of the track 3 minutes 30 seconds.
            int count = (int)Math.Round(totalTrackLength / minSizeChunk), fingerprintsCount = 0, queryMatchLength = 10, ongoingCalls = 0;
            var data = GenerateRandomAudioChunks(count, seed: 1, DateTime.UtcNow);
            var concatenated = Concatenate(data);
            var avHashes = await FingerprintCommandBuilder.Instance
                                                .BuildFingerprintCommand()
                                                .From(concatenated)
                                                .Hash();

            Assert.NotNull(avHashes.Audio);
            
            // hashes have to be equal to total track length +- 1 second
            Assert.AreEqual(totalTrackLength, avHashes.Audio.DurationInSeconds, delta: 1);
            
            // store track data and associated hashes
            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen"), avHashes);

            var successMatches = new List<ResultEntry>();
            var didNotGetToContiguousQueryMatchLengthMatch = new List<ResultEntry>();

            // simulating realtime query, starting in the middle of the track ~1 min 45 seconds (105 seconds).
            // and we query for 35 seconds
            const double queryLength = 35;
            // track  ---------------------------- 210 seconds
            // query                ----           35 seconds
            // match starts at      |              105 second
            var realtimeQuery = data.Skip(count / 2).Take((int)(queryLength/minSizeChunk) + 1).ToArray();

            double duration = realtimeQuery.Sum(_ => _.Duration);
            Assert.AreEqual(queryLength, duration, 1); // asserting the total length of the query +- 1 second
            
            // adding some jitter before and after the query which should not match
            // track           ---------------------------- 210 seconds
            // q with jitter             ^^^----^^^         10 sec + 35 seconds + 10 sec = 55 sec
            // match starts at              |               105 second
            const double jitterLength = 10;
            var collection = SimulateRealtimeAudioQueryData(realtimeQuery, jitterLength);
            double processed = await QueryCommandBuilder.Instance
                                            .BuildRealtimeQueryCommand()
                                            .From(collection)
                                            .WithRealtimeQueryConfig(config =>
                                            {
                                                config.QueryConfiguration.Audio.Stride = new IncrementalRandomStride(256, 512);
                                                config.QueryConfiguration.Audio.PermittedGap = 2;
                                                config.ResultEntryFilter = new TrackCoverageLengthEntryFilter(queryMatchLength, waitTillCompletion: false);
                                                config.OngoingResultEntryFilter = new ChainedRealtimeEntryFilter([
                                                    new TrackRelativeCoverageEntryFilter(0.2d, waitTillCompletion: false), 
                                                    new TrackCoverageLengthEntryFilter(1d, waitTillCompletion: false)
                                                ]);
                                                config.SuccessCallback = result =>
                                                {
                                                    foreach (var (entry, _) in result.ResultEntries)
                                                    {
                                                        Assert.That(entry, Is.Not.Null);
                                                        logger.LogInformation("Found Match Starts At {TrackMatchStartsAt}, Match Length {TrackCoverageWithPermittedGapsLength}, Query Length {QueryLength} Track Starts At {TrackStartsAt}", entry.TrackMatchStartsAt, entry.TrackCoverageWithPermittedGapsLength, entry.QueryLength, entry.TrackStartsAt);
                                                        successMatches.Add(entry);
                                                    }
                                                };

                                                config.DidNotPassFilterCallback = result =>
                                                {
                                                    foreach (var (entry, _) in result.ResultEntries)
                                                    {
                                                        Assert.That(entry, Is.Not.Null);
                                                        logger.LogInformation("Entry didn't pass filter, Starts At {TrackMatchStartsAt}, Match Length {TrackCoverageWithPermittedGapsLength}, Query Length {QueryLength}", entry.TrackMatchStartsAt, entry.TrackCoverageWithPermittedGapsLength, entry.QueryLength);
                                                        didNotGetToContiguousQueryMatchLengthMatch.Add(entry);
                                                    }
                                                };

                                                config.OngoingCallback = _ => { Interlocked.Add(ref ongoingCalls, _.Count()); };
                                                config.ErrorCallback = (error, _) => throw error;
                                                config.RestoredAfterErrorCallback = () => throw new Exception("Downtime callback called");
                                                return config;
                                            })
                                            .InterceptHashes(fingerprints =>
                                            {
                                                Interlocked.Add(ref fingerprintsCount, fingerprints.Audio?.Count + fingerprints.Video?.Count ?? 0);
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
            Assert.AreEqual(queryLength + 2 * jitterLength, processed, 3);

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
            Assert.AreEqual(realtimeQuery.Length + 6 /*jitter call since track can continue in the next query*/, ongoingCalls);
        }

        [Test]
        public async Task ShouldNotLoseAudioSamplesInCaseIfExceptionIsThrown()
        {
            var modelService = new InMemoryModelService();

            double minSizeChunk = (double)minSamplesPerFingerprint / SampleRate; // length in seconds of one query chunk ~1.8577
            const double totalTrackLength = 210;                                 // length of the track 3 minutes 30 seconds.
            const double jitterLength = 10;
            double totalQueryLength = totalTrackLength + 2 * jitterLength;
            
            int trackCount = (int)(totalTrackLength / minSizeChunk), fingerprintsCount = 0, avTracksCount = 0, errored = 0, didNotPassThreshold = 0, jitterChunks = 2 * (int)Math.Ceiling(jitterLength * 5512 / minSamplesPerFingerprint);
            
            var start = new DateTime(2021, 5, 1, 0, 0, 0, DateTimeKind.Utc);
            var data = GenerateRandomAudioChunks(trackCount, seed: 1, start);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                                                .BuildFingerprintCommand()
                                                .From(concatenated)
                                                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen"), hashes);

            var resultEntries = new List<AVResultEntry>();
            var collection = SimulateRealtimeAudioQueryData(data, jitterLength);
            var offlineStorage = new OfflineStorage(Path.GetTempPath());
            var restoreCalled = new bool[1];
            var loggerFactory = new NullLoggerFactory();
            double processed = await new RealtimeQueryCommand(FingerprintCommandBuilder.Instance, new QueryCommandBuilder(FingerprintCommandBuilder.Instance, 
                    new FaultyQueryService(faultyCounts: trackCount + jitterChunks - 1, QueryFingerprintService.Instance), loggerFactory), loggerFactory)
                 .From(collection)
                 .WithRealtimeQueryConfig(config =>
                 {
                     config.SuccessCallback = entry => resultEntries.AddRange(entry.ResultEntries);
                     config.DidNotPassFilterCallback = _ => Interlocked.Increment(ref didNotPassThreshold);
                     config.ErrorCallback = (_, _) => Interlocked.Increment(ref errored);
                     config.ResultEntryFilter = new TrackRelativeCoverageEntryFilter(0.4, waitTillCompletion: true);
                     config.RestoredAfterErrorCallback = () => restoreCalled[0] = true;
                     config.OfflineStorage = offlineStorage;                            // store the other half of the fingerprints in the downtime hashes storage
                     config.DelayStrategy = new NoDelayStrategy();
                     return config;
                 })
                 .InterceptAVTrack(tracks =>
                 {
                     Interlocked.Increment(ref avTracksCount);
                     return tracks;
                 })
                 .InterceptHashes(fingerprints =>
                 {
                     Interlocked.Increment(ref fingerprintsCount);
                     return fingerprints;
                 })
                 .UsingServices(modelService)
                 .Query(CancellationToken.None);

            Assert.AreEqual(totalQueryLength, processed, 3);
            Assert.AreEqual(trackCount + jitterChunks - 1, errored);
            Assert.AreEqual(trackCount, avTracksCount - jitterChunks);
            Assert.AreEqual(trackCount, fingerprintsCount - jitterChunks);
            Assert.IsTrue(restoreCalled[0]);
            Assert.AreEqual(1, resultEntries.Count);
            var (result, _) = resultEntries.First();
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(totalTrackLength, result.Coverage.TrackCoverageWithPermittedGapsLength, 2 * minSizeChunkDuration);
            Assert.IsTrue(Math.Abs(start.Subtract(result.MatchedAt).TotalSeconds) < 2, $"Matched At {result.MatchedAt:o}");
            Assert.AreEqual(0, didNotPassThreshold);
        }

        [Test]
        public async Task HashesShouldMatchExactlyWhenAggregated()
        {
            var modelService = new InMemoryModelService();

            int count = 20;
            var data = GenerateRandomAudioChunks(count, seed: 1, DateTime.UtcNow);
            var concatenated = Concatenate(data);
            var (hashes, _) = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(concatenated)
                .WithFingerprintConfig(config =>
                {
                    config.Audio.Stride = new IncrementalStaticStride(512);
                    return config;
                })
                .Hash();
            
            var collection = SimulateRealtimeAudioQueryData(data, jitterLength: 0);
            var list = new List<AVHashes>();
            
            await QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                .From(collection)
                .WithRealtimeQueryConfig(config =>
                {
                    config.QueryConfiguration.Audio.Stride = new IncrementalStaticStride(512);
                    return config;
                })
                .InterceptHashes(timedHashes =>
                {
                    list.Add(timedHashes);
                    return timedHashes;
                })
                .UsingServices(modelService)
                .Query(CancellationToken.None);
            
            Assert.That(hashes, Is.Not.Null);
            Assert.AreEqual(hashes.Count, list.Select(entry => entry.Audio?.Count).Sum());
            var merged = Hashes.Aggregate(list.Select(_ => _.Audio), concatenated.Duration).ToList();
            Assert.AreEqual(1, merged.Count, $"Hashes:{string.Join(",", merged.Select(_ => $"{_.RelativeTo},{_.DurationInSeconds:0.00}"))}");
            Assert.AreEqual(hashes.Count, merged.Select(entry => entry.Count).Sum());

            var aggregated = Hashes.Aggregate(list.Select(_ => _.Audio), double.MaxValue).ToList();
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
            var modelService = new InMemoryModelService();

            int count = 20, testWaitTime = 5000;
            var data = GenerateRandomAudioChunks(count, 1, DateTime.UtcNow);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(concatenated)
                .WithFingerprintConfig(config => config)
                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen"), hashes);

            var collection = SimulateRealtimeAudioQueryData(data, jitterLength: 0);
            var cancellationTokenSource = new CancellationTokenSource(testWaitTime);
            var fingerprints = new List<AVHashes>();
            var entries = new List<AVResultEntry>();
            
            await QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                .From(collection)
                .WithRealtimeQueryConfig(config =>
                {
                    config.SuccessCallback = entry => entries.AddRange(entry.ResultEntries);
                    config.ResultEntryFilter = new TrackRelativeCoverageEntryFilter(0.8d, waitTillCompletion: true);
                    config.QueryConfiguration.Audio.Stride = new IncrementalStaticStride(2048);
                    return config;
                })
                .InterceptHashes(queryHashes =>
                {
                    fingerprints.Add(queryHashes);
                    return queryHashes;
                })
                .UsingServices(modelService)
                .Query(cancellationTokenSource.Token);

            Assert.IsTrue(entries.Any());
            Assert.AreEqual(1, entries.Count);
            var (realtimeResult, _) = entries.First();
            var aggregatedHashes = Hashes.Aggregate(fingerprints.Select(_ => _.Audio), 60d).First();
            var (nonRealtimeResult, _) = await QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(new AVHashes(aggregatedHashes, null))
                .UsingServices(modelService)
                .Query();
            
            Assert.That(nonRealtimeResult, Is.Not.Null);
            Assert.IsTrue(nonRealtimeResult.ContainsMatches);
            Assert.AreEqual(1, nonRealtimeResult.ResultEntries.Count());
            Assert.That(realtimeResult, Is.Not.Null);
            Assert.AreEqual(realtimeResult.MatchedAt, aggregatedHashes.RelativeTo);
            Assert.AreEqual(realtimeResult.MatchedAt, nonRealtimeResult.BestMatch?.MatchedAt, $"Realtime vs NonRealtime {nonRealtimeResult.BestMatch?.Coverage.BestPath.Count()} match time does not match");
        }

        [Test]
        public async Task ShouldPurgeCompletedMatchWhenAsyncCollectionIsExhausted()
        {
            var modelService = new InMemoryModelService();

            const double totalTrackLength = 210;       // length of the track 3 minutes 30 seconds.

            var data = GenerateRandomAudioChunks((int)(totalTrackLength / minSizeChunkDuration), seed: 1, DateTime.UtcNow);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(concatenated)
                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen"), hashes);

            var success = new List<AVResultEntry>();
            var didNotPass = new List<AVResultEntry>();
            await QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                .From(SimulateRealtimeAudioQueryData(data, jitterLength: 0, seed: 5566))
                .WithRealtimeQueryConfig(config =>
                {
                    config.QueryConfiguration.Audio.Stride = new IncrementalRandomStride(256, 512, seed: 123);
                    config.ResultEntryFilter = new TrackRelativeCoverageEntryFilter(0.5, waitTillCompletion: true);
                    config.SuccessCallback = _ => success.AddRange(_.ResultEntries);
                    config.DidNotPassFilterCallback = _ => didNotPass.AddRange(_.ResultEntries);
                    return config;
                })
                .UsingServices(modelService)
                .Query(CancellationToken.None);

            Assert.AreEqual(0, didNotPass.Count);
            Assert.AreEqual(1, success.Count);
        }

        [Test]
        public async Task ShouldContinueQueryingEvenWhenAnErrorOccurs()
        {
            const int length = 60;
            const int throwExceptionAfter = 3;
            const int totalExceptions = 5;
            int exceptions = 0, restored = 0;
            var cancellationTokenSource = new CancellationTokenSource();
            var indefiniteSource = GetSamplesIndefinitely(length, throwExceptionAfter);
            var backoffPolicy = new Mock<IBackoffPolicy>(MockBehavior.Strict);
            backoffPolicy.Setup(p => p.RemainingDelay).Returns(TimeSpan.Zero);
            backoffPolicy.Setup(p => p.Failure());
            backoffPolicy.Setup(p => p.Success());
            
            var modelService = new Mock<IModelService>(MockBehavior.Strict);
            modelService.Setup(s => s.QueryEfficiently(It.IsAny<Hashes>(), It.IsAny<QueryConfiguration>())).Returns(new Candidates());

            var queryLength = await QueryCommandBuilder.Instance
                .BuildRealtimeQueryCommand()
                .From(indefiniteSource)
                .WithRealtimeQueryConfig(config =>
                {
                    config.ErrorCallback = (_, _) =>
                    {
                        exceptions++;
                        if (exceptions == totalExceptions)
                        {
                            cancellationTokenSource.Cancel();
                        }
                    };

                    config.RestoredAfterErrorCallback = () => restored++;
                    config.ErrorBackoffPolicy = backoffPolicy.Object;
                    return config;
                })
                .UsingServices(modelService.Object)
                .Query(cancellationTokenSource.Token);

            int totalQueries = throwExceptionAfter * totalExceptions - totalExceptions;
            
            Assert.AreEqual(totalExceptions, exceptions);
            Assert.AreEqual(totalExceptions - 1, restored);
            Assert.AreEqual(totalQueries * length, queryLength);
            
            modelService.Verify(s => s.QueryEfficiently(It.IsAny<Hashes>(), It.IsAny<QueryConfiguration>()), Times.Exactly(totalQueries));
            backoffPolicy.Verify(b => b.Failure(), Times.Exactly(totalExceptions));
            backoffPolicy.Verify(b => b.RemainingDelay, Times.Exactly(totalExceptions));
            backoffPolicy.Verify(b => b.Success(), Times.Exactly(totalExceptions - 1));
        }

        [Test]
        public async Task ShouldQueryBothAudioAndVideo()
        {
            var modelService = new InMemoryModelService();

            double minAudioSizeChunk = (double)minSamplesPerFingerprint / SampleRate;
            const double totalTrackLength = 210;      
            int audioCount = (int)Math.Round(totalTrackLength / minAudioSizeChunk), fingerprintsCount = 0, ongoingCalls = 0;
            var relativeTo = DateTime.UtcNow;
            var audioData = GenerateRandomAudioChunks(audioCount, seed: 123, relativeTo);
            var concatenatedAudio = Concatenate(audioData);
            var videoData = GenerateRandomFrameChunks(audioCount, seed: 456, relativeTo);
            var concatenatedVideo = Concatenate(videoData);
            Assert.AreEqual(concatenatedAudio.Duration, concatenatedVideo.Duration, (double)minSamplesPerFingerprint / SampleRate);

            var avHashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(new AVTrack(new AudioTrack(concatenatedAudio), new VideoTrack(concatenatedVideo)))
                .Hash();
            
            Assert.That(avHashes.Audio, Is.Not.Null);
            Assert.That(avHashes.Video, Is.Not.Null);
            Assert.That(avHashes.Audio.RelativeTo, Is.EqualTo(avHashes.Video.RelativeTo));
            
            modelService.Insert(new TrackInfo("1", string.Empty, string.Empty, MediaType.Audio | MediaType.Video), avHashes);

            var avTracks = audioData.Zip(videoData).Select(_ => new AVTrack(new AudioTrack(_.First), new VideoTrack(_.Second))).ToList();
            int jitterLength = 10, successCallbackCalls = 0;
            var collection = SimulateRealtimeAudioVideoQueryData(avTracks, jitterLength, seed: 789);
            
            var successMatches = new List<AVResultEntry>();
            var didNotGetToContiguousQueryMatchLengthMatch = new List<AVResultEntry>();
            double processed = await QueryCommandBuilder
                                            .Instance
                                            .BuildRealtimeQueryCommand()
                                            .From(collection)
                                            .WithRealtimeQueryConfig(config =>
                                            {
                                                config.QueryConfiguration.Audio.Stride = new IncrementalRandomStride(256, 512, seed: 123);
                                                config.ResultEntryFilter = new CompletedRealtimeMatchResultEntryFilter();
                                                config.OngoingResultEntryFilter = new ChainedRealtimeEntryFilter([
                                                    new TrackRelativeCoverageEntryFilter(0.2d, waitTillCompletion: false),
                                                    new TrackCoverageLengthEntryFilter(2d, waitTillCompletion: false)
                                                ]);
                                                config.SuccessCallback = result =>
                                                {
                                                    successCallbackCalls++;
                                                    successMatches.AddRange(result.ResultEntries);
                                                };

                                                config.DidNotPassFilterCallback = result =>
                                                {
                                                    didNotGetToContiguousQueryMatchLengthMatch.AddRange(result.ResultEntries);
                                                };

                                                config.OngoingCallback = _ =>
                                                {
                                                    Interlocked.Add(ref ongoingCalls, _.Count());
                                                };
                                                
                                                config.ErrorCallback = (error, _) => throw error;
                                                return config;
                                            })
                                            .InterceptHashes(fingerprints =>
                                            {
                                                Interlocked.Add(ref fingerprintsCount, fingerprints.Audio?.Count + fingerprints.Video?.Count ?? 0);
                                                return fingerprints;
                                            })
                                            .UsingServices(modelService)
                                            .Query(CancellationToken.None);
            
            Assert.AreEqual(1, successMatches.Count, $"There should be only one match:\n{string.Join("\n", successMatches)}\nSuccess Callback Calls: {successCallbackCalls}");
            var (audioResult, videoResult) = successMatches.First();
            Assert.IsNotNull(audioResult);
            Assert.IsNotNull(videoResult);
            Assert.AreEqual(1, audioResult.Confidence, 0.01);
            Assert.AreEqual(1, audioResult.Coverage.TrackRelativeCoverage, 0.01);
            Assert.AreEqual(1, videoResult.Confidence, 0.01);
            Assert.AreEqual(1, videoResult.Coverage.TrackRelativeCoverage, 0.01);
            Assert.AreEqual(0, didNotGetToContiguousQueryMatchLengthMatch.Count);
            Assert.AreEqual(avTracks.Count, ongoingCalls, 1);
            Assert.AreEqual(totalTrackLength + jitterLength + jitterLength, processed, 3d);
        }

        [Test]
        public async Task ShouldReadDirectlyFromMediaService()
        {
            var cancellationToken = CancellationToken.None;
            var realtimeMediaService = new Mock<IRealtimeMediaService>();
            var modelService = new InMemoryModelService();
            const int sampleRate = 8192;
            realtimeMediaService.Setup(_ => _.ReadAVTrackFromRealtimeSource("http://localhost", 60, It.IsAny<AVTrackReadConfiguration>(), MediaType.Audio, cancellationToken))
                .Callback((string _, double _, AVTrackReadConfiguration avTrackConfig, MediaType _, CancellationToken _) =>
                {
                     Assert.AreEqual(sampleRate, avTrackConfig.AudioConfig.SampleRate);
                })
                .Returns(GetSamples(10, 60, cancellationToken, sampleRate: sampleRate));

           var length = await  QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                .From("http://localhost", 60, mediaType: MediaType.Audio)
                .WithRealtimeQueryConfig(config =>
                {
                    config.QueryConfiguration.FingerprintConfiguration.Audio.SampleRate = sampleRate;
                    return config;
                })
                .UsingServices(modelService, realtimeMediaService.Object)
                .Query(cancellationToken);
           
           realtimeMediaService.Verify(_ => _.ReadAVTrackFromRealtimeSource("http://localhost", 60, It.IsAny<AVTrackReadConfiguration>(), MediaType.Audio, cancellationToken), Times.Exactly(1));
           Assert.AreEqual(60 * 10, length);
        }

        [Test]
        public async Task ShouldCancelReadingFromUrl()
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var realtimeMediaService = new Mock<IRealtimeMediaService>();
            var modelService = new InMemoryModelService();
            realtimeMediaService
                .Setup(_ => _.ReadAVTrackFromRealtimeSource("http://localhost", 60, It.IsAny<AVTrackReadConfiguration>(), MediaType.Audio, It.IsAny<CancellationToken>()))
                .Returns((string _, double _, AVTrackReadConfiguration _, MediaType _, CancellationToken cancellationToken) => GetSamples(10, 60, cancellationToken, sampleRate: SampleRate, delay: 1));

            var queryCommandBuilder = new QueryCommandBuilder(new NullLoggerFactory());
            var length = await queryCommandBuilder.BuildRealtimeQueryCommand()
                .From("http://localhost", 60, mediaType: MediaType.Audio)
                .UsingServices(modelService, realtimeMediaService.Object)
                .Query(cancellationTokenSource.Token);
           
            realtimeMediaService.Verify(_ => _.ReadAVTrackFromRealtimeSource("http://localhost", 60, It.IsAny<AVTrackReadConfiguration>(), MediaType.Audio, It.IsAny<CancellationToken>()), Times.Exactly(1));
            Assert.IsTrue(length <= 180, $"Length {length} is bigger than 180");
        }

        [Test]
        public async Task ShouldHandleRealtimeConnectionException()
        {
            var realtimeMediaService = new Mock<IRealtimeMediaService>();
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var modelService = new InMemoryModelService();
            bool errored = false;
            realtimeMediaService.Setup(_ => _.ReadAVTrackFromRealtimeSource("url", It.IsAny<double>(), It.IsAny<AVTrackReadConfiguration>(), MediaType.Audio, It.IsAny<CancellationToken>())).Throws(new ApplicationException("Could not connect to provided URL"));
            await QueryCommandBuilder.Instance
                .BuildRealtimeQueryCommand()
                .From("url", 60, mediaType: MediaType.Audio)
                .WithRealtimeQueryConfig(config =>
                {
                    config.ErrorCallback = (ex, _) =>
                    {
                        Assert.IsInstanceOf<ApplicationException>(ex);
                        errored = true;
                    };
                
                    return config;
                })
                .UsingServices(modelService, realtimeMediaService.Object)
                .Query(cancellationTokenSource.Token);
            
            Assert.IsTrue(errored);
        }

        [Test]
        public async Task ShouldHandleExceptionInRealtimeSource()
        {
            var modelService = new InMemoryModelService();

            int count = 20;
            var data = GenerateRandomAudioChunks(count, seed: 1, DateTime.UtcNow);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder
                .Instance
                .BuildFingerprintCommand()
                .From(concatenated)
                .WithFingerprintConfig(config => config)
                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen"), hashes);

            var collection = SimulateRealtimeAudioQueryData(data, jitterLength: 10, seed: 8877);
            var withFailure = SimulateFailure(collection, throwExceptionAfter: 15);
            
            var success = new List<AVResultEntry>();
            var didNotPass = new List<AVResultEntry>();
            bool receivedSuccessBeforeEnd = false;
            await QueryCommandBuilder.Instance
                .BuildRealtimeQueryCommand()
                .From(withFailure)
                .WithRealtimeQueryConfig(config =>
                {
                    config.QueryConfiguration.Audio.Stride = new IncrementalRandomStride(256, 512, seed: 123);
                    config.ResultEntryFilter = new TrackRelativeCoverageEntryFilter(coverage: 0.2, waitTillCompletion: true);
                    config.SuccessCallback = _ => success.AddRange(_.ResultEntries);
                    config.DidNotPassFilterCallback = _ => didNotPass.AddRange(_.ResultEntries);
                    config.ErrorCallback = (e, _) => logger.LogError(e, "An error occured while querying stream");
                    return config;
                })
                .InterceptHashes(avHashes =>
                {
                    if (success.Any())
                    {
                        receivedSuccessBeforeEnd = true;
                    }

                    return avHashes;
                })
                .UsingServices(modelService)
                .Query(CancellationToken.None);

            Assert.That(success.Count, Is.EqualTo(1));
            Assert.That(didNotPass.Count, Is.Zero, "Found matches:\n" + string.Join("\n", didNotPass));
            var entry = success.First();
            Assert.That(entry.Audio, Is.Not.Null);
            Assert.That(entry.Video, Is.Null);
            Assert.That(entry.Audio.Coverage.TrackCoverageWithPermittedGapsLength, Is.EqualTo(hashes.Audio!.DurationInSeconds).Within(1d));
            Assert.That(receivedSuccessBeforeEnd, Is.True);
        }
        
        private static async IAsyncEnumerable<AudioSamples> SimulateFailure(IAsyncEnumerable<AudioSamples> collection, int throwExceptionAfter)
        {
            await foreach (var audioSamples in collection)
            {
                if (throwExceptionAfter-- == 0)
                {
                    throw new Exception("Error while reading samples from the source.");
                }

                yield return audioSamples;
            }
        }

        private static async IAsyncEnumerable<AVTrack> GetSamples(int count, int seconds, [EnumeratorCancellation] CancellationToken cancellationToken, int sampleRate = 5512, int delay = 0)
        {
            foreach (var track in Enumerable.Range(0, count).Select(_ => new AVTrack(new AudioTrack(TestUtilities.GenerateRandomAudioSamples(seconds * sampleRate, sampleRate: sampleRate)), null)))
            {
                yield return track;
                await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);
            }
        }

        private static async IAsyncEnumerable<AudioSamples> GetSamplesIndefinitely(int eachLengthSeconds, int throwExceptionAfter)
        {
            int count = 0;
            while (true)
            {
                count++;
                if (count % throwExceptionAfter == 0)
                {
                    throw new Exception("Error while reading samples from the source.");
                }

                await Task.Delay(TimeSpan.Zero);
                yield return TestUtilities.GenerateRandomAudioSamples(eachLengthSeconds * 5512);
            }
        }

        private static Frames Concatenate(IEnumerable<Frames> data)
        {
            IEnumerable<Frames> enumerable = data as Frames[] ?? data.ToArray();
            var frames = enumerable.SelectMany(_ => _).Select((frame, index) => new Frame(frame.ImageRowCols, frame.Rows, frame.Cols, (float)index / 30, (uint)index));
            return new Frames(frames, string.Empty, 30, enumerable.First().RelativeTo);
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

        private List<AudioSamples> GenerateRandomAudioChunks(int count, int seed, DateTime relativeTo)
        {
            return Enumerable
                .Range(0, count)
                .Select(index => GetMinSizeOfAudioSamples(seed * index, relativeTo.AddSeconds(index * minSizeChunkDuration)))
                .ToList();
        }

        private List<Frames> GenerateRandomFrameChunks(int count, int seed, DateTime relativeTo)
        {
            int frameRate = 30;
            return Enumerable
                .Range(0, count)
                .Select(index =>
                {
                    var frames = Enumerable.Range(0, (int)(minSizeChunkDuration * frameRate)).Select(chunkIndex => new Frame(TestUtilities.GenerateRandomFloatArray(128 * 72, index * seed).Select(_ => _ / 32767).ToArray(), 128, 72, ((float)chunkIndex) / 30, (uint)chunkIndex)).ToList();
                    return new Frames(frames, string.Empty, frameRate: frameRate, relativeTo.AddSeconds((float)(index * frames.Count) / frameRate));
                }).ToList();
        }

        private IAsyncEnumerable<AVTrack> SimulateRealtimeAudioVideoQueryData(IReadOnlyCollection<AVTrack> existingTrack, double jitterLength, int seed = 0)
        {
            var collection = new BlockingCollection<AVTrack>();
            Task.Factory.StartNew(() =>
            {
                if (jitterLength > 0)
                {
                    var relativeTo = existingTrack.First()?.Audio?.Samples.RelativeTo.AddSeconds(-jitterLength) ?? DateTime.UtcNow;
                    Jitter(collection, jitterLength, relativeTo, seed);
                }

                foreach (var avTrack in existingTrack)
                {
                    collection.Add(avTrack);
                }

                if (jitterLength > 0)
                {
                    var relativeTo = existingTrack.Last()?.Audio?.Samples.RelativeTo ?? DateTime.UtcNow;
                    Jitter(collection, jitterLength, relativeTo, 2 * seed);
                }

                collection.CompleteAdding();
            });

            return new BlockingRealtimeCollection<AVTrack>(collection); 
        }

        /// <summary>
        ///  Simulate realtime query data.
        /// </summary>
        /// <param name="audioSamples">Chunks of audio samples.</param>
        /// <param name="jitterLength">Jitter length in seconds, added at the beginning and at the end.</param>
        /// <param name="seed">Random seed.</param>
        /// <returns>Async enumerable.</returns>
        private IAsyncEnumerable<AudioSamples> SimulateRealtimeAudioQueryData(IReadOnlyCollection<AudioSamples> audioSamples, double jitterLength, int seed = 0)
        {
            var collection = new BlockingCollection<AudioSamples>();
            Task.Factory.StartNew(() =>
            {
                if (jitterLength > 0)
                {
                    var startAt = audioSamples.First()?.RelativeTo.AddSeconds(-jitterLength) ?? DateTime.UtcNow;
                    Jitter(collection, jitterLength, startAt, seed);
                }

                foreach (var audioSample in audioSamples)
                {
                    collection.Add(new AudioSamples(audioSample.Samples, audioSample.Origin, audioSample.SampleRate, audioSample.RelativeTo));
                }

                if (jitterLength > 0)
                {
                    var endsAt = audioSamples.Last()?.RelativeTo ?? DateTime.UtcNow;
                    Jitter(collection, jitterLength, endsAt, seed);
                }

                collection.CompleteAdding();
            });

            return new BlockingRealtimeCollection<AudioSamples>(collection);
        }

        private void Jitter(BlockingCollection<AVTrack> collection, double jitterLength, DateTime relativeTo, int seed = 0)
        {
            seed = seed == 0 ? Random.Shared.Next() : seed;
            int count = (int)Math.Ceiling(jitterLength * 5512 / minSamplesPerFingerprint);
            var audioSamples = GenerateRandomAudioChunks(count, seed, relativeTo);
            var videoFrames = GenerateRandomFrameChunks(count, seed, relativeTo);
            var avTracks = audioSamples.Zip(videoFrames).Select(_ => new AVTrack(new AudioTrack(_.First), new VideoTrack(_.Second))).ToList();
            foreach (var avTrack in avTracks)
            {
                collection.Add(avTrack);
            }
        }

        private void Jitter(BlockingCollection<AudioSamples> collection, double jitterLength, DateTime relativeTo, int seed = 0)
        {
            seed = seed == 0 ? Random.Shared.Next() : seed;
            int count = (int)Math.Ceiling(jitterLength * 5512 / minSamplesPerFingerprint);
            var audioSamples = GenerateRandomAudioChunks(count, seed, relativeTo); 
            foreach (var audioSample in audioSamples)
            {
                collection.Add(audioSample);
            }
        }

        private AudioSamples GetMinSizeOfAudioSamples(int seed, DateTime relativeTo)
        {
            var samples = TestUtilities.GenerateRandomFloatArray(minSamplesPerFingerprint, seed);
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
            
            public QueryResult Query(Hashes queryFingerprints, QueryConfiguration configuration, IQueryService queryService)
            {
                if (faultyCounts-- > 0)
                {
                    throw new IOException("I/O exception");
                }

                return goodOne.Query(queryFingerprints, configuration, queryService);
            }
        }
    }
}