namespace SoundFingerprinting.Tests.Unit.Command
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using NUnit.Framework;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Content;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Strides;

    [TestFixture]
    public class RealtimeFingerprintCommandTest
    {
        private const int SampleRate = 5512;
        private static readonly int MinSamples = new DefaultAVFingerprintConfiguration().Audio.SpectrogramConfig.MinimumSamplesPerFingerprint;
        private static readonly DateTime Start = new (2026, 10, 9, 12, 0, 0, DateTimeKind.Utc);

        [Test]
        public async Task RealtimeQueryCommandShouldForwardSourceConfigurationAndInterceptorsToTheFingerprintCommand()
        {
            // the query command consumes this command, so equal hashes prove the forwarding; the hashing gate is HashesShouldMatchExactlyWhenAggregated
            var chunks = Chunks(20, index => Expected(index));

            var fromFingerprintCommand = await Hash(Cold(chunks), config =>
            {
                config = new DefaultAVQueryConfiguration().FingerprintConfiguration;
                config.Audio.Stride = new IncrementalStaticStride(512);
                return config;
            });

            var fromQueryCommand = new List<AVHashes>();
            await QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                .From(Cold(chunks))
                .WithRealtimeQueryConfig(config =>
                {
                    config.QueryConfiguration.Audio.Stride = new IncrementalStaticStride(512);
                    return config;
                })
                .InterceptHashes(hashes =>
                {
                    fromQueryCommand.Add(hashes);
                    return hashes;
                })
                .UsingServices(new InMemoryModelService())
                .Query(CancellationToken.None);

            Assert.That(fromFingerprintCommand, Has.Count.EqualTo(chunks.Count));
            Assert.That(fromQueryCommand, Has.Count.EqualTo(fromFingerprintCommand.Count));
            foreach (var (left, right) in fromFingerprintCommand.Zip(fromQueryCommand))
            {
                Assert.Multiple(() =>
                {
                    Assert.That(right.RelativeTo, Is.EqualTo(left.RelativeTo));
                    Assert.That(right.Audio!.TimeOffset, Is.EqualTo(left.Audio!.TimeOffset));
                    // the hasher emits fingerprints in parallel, so the order inside a chunk is not stable
                    Assert.That(Bins(right), Is.EqualTo(Bins(left)).AsCollection);
                });
            }
        }

        [Test]
        public async Task ShouldKeepTheSourceStampsWithoutAnInterceptor()
        {
            var random = new Random(7);
            var chunks = Chunks(20, index => Expected(index).AddMilliseconds(random.Next(-400, 400)));

            var hashes = await Hash(Cold(chunks));

            Assert.That(hashes.Select(_ => _.RelativeTo), Is.EqualTo(chunks.Select(_ => _.RelativeTo)).AsCollection);
            Assert.That(hashes[0].Audio!.TimeOffset, Is.EqualTo(0));
            Assert.That(hashes.Skip(1).Select(_ => _.Audio!.TimeOffset), Has.All.LessThan(0), "the tail of the previous chunk is carried in TimeOffset");
        }

        [Test]
        public async Task ShouldHashAVideoOnlySource()
        {
            var chunks = Enumerable.Range(0, 10)
                .Select(index => new AVTrack(null, new VideoTrack(TestUtilities.GenerateRandomFrames(60, Start.AddSeconds(2 * index)))))
                .ToList();

            var hashes = new List<AVHashes>();
            await foreach (var entry in FingerprintCommandBuilder.Instance
                .BuildRealtimeFingerprintCommand()
                .From(Cold(chunks))
                .Hash(CancellationToken.None))
            {
                hashes.Add(entry);
            }

            Assert.That(hashes, Has.Count.EqualTo(chunks.Count));
            Assert.Multiple(() =>
            {
                Assert.That(hashes.Select(_ => _.Audio), Has.All.Null);
                Assert.That(hashes.Select(_ => _.Video!.Count), Has.All.EqualTo(60));
                Assert.That(hashes.Select(_ => _.RelativeTo), Is.EqualTo(chunks.Select(_ => _.RelativeTo)).AsCollection);
            });
        }

        [Test]
        public async Task ReEnumerationShouldKeepTheTail()
        {
            var chunks = Chunks(10, index => Expected(index));
            var oneRun = await Hash(Cold(chunks));

            var collection = new BlockingCollection<AudioSamples>();
            foreach (var chunk in chunks)
            {
                collection.Add(chunk);
            }

            collection.CompleteAdding();
            var hot = FingerprintCommandBuilder.Instance
                .BuildRealtimeFingerprintCommand()
                .From(new BlockingRealtimeCollection<AudioSamples>(collection, TimeSpan.FromMilliseconds(10)))
                .Hash(CancellationToken.None);

            var resumed = new List<AVHashes>();
            await foreach (var hashes in hot)
            {
                resumed.Add(hashes);
                if (resumed.Count == 3)
                {
                    // a transient error in the consumer: the enumeration stops and starts again on the same enumerable
                    break;
                }
            }

            await foreach (var hashes in hot)
            {
                resumed.Add(hashes);
            }

            Assert.That(resumed, Has.Count.EqualTo(oneRun.Count));
            foreach (var (expected, actual) in oneRun.Zip(resumed))
            {
                Assert.Multiple(() =>
                {
                    Assert.That(actual.RelativeTo, Is.EqualTo(expected.RelativeTo));
                    Assert.That(actual.Audio!.TimeOffset, Is.EqualTo(expected.Audio!.TimeOffset), "the buffered tail survives the re-enumeration");
                    Assert.That(Bins(actual), Is.EqualTo(Bins(expected)).AsCollection);
                });
            }
        }

        [Test]
        public async Task AVTrackInterceptorShouldFireForEveryChunkIncludingShortOnesAndItsStampShouldWin()
        {
            int half = MinSamples / 2;
            var chunks = Enumerable.Range(0, 10)
                .Select(index => new AudioSamples(TestUtilities.GenerateRandomFloatArray(half, index + 1), "cnn", SampleRate, Start.AddSeconds(index)))
                .ToList();

            var shift = TimeSpan.FromSeconds(100);
            var intercepted = new List<DateTime>();
            var hashes = await Hash(Cold(chunks), avTrackInterceptor: track =>
            {
                intercepted.Add(track.RelativeTo);
                return track.WithRelativeTo(track.RelativeTo + shift);
            });

            Assert.That(intercepted, Is.EqualTo(chunks.Select(_ => _.RelativeTo)).AsCollection, "the interceptor fires for a chunk that is too short to yield hashes");
            Assert.That(hashes, Has.Count.LessThan(chunks.Count));
            Assert.That(hashes.Select(_ => _.RelativeTo), Has.All.GreaterThanOrEqualTo(Start + shift), "the hashes take the stamp the interceptor returned");
        }

        [Test]
        public async Task StreamingFilesShouldCarryTheStreamIdAndTheFileStampIntoTheInterceptor()
        {
            var random = new Random(3);
            var chunks = Chunks(8, index => Expected(index).AddMilliseconds(random.Next(-400, 400)));
            var audioService = new Mock<IAudioService>(MockBehavior.Strict);
            var files = new List<StreamingFile>();
            for (int index = 0; index < chunks.Count; ++index)
            {
                string path = $"chunk_{index}.wav";
                // the reader stamps the samples with a clock of its own, the file stamp has to win
                audioService.Setup(service => service.ReadMonoSamplesFromFile(path, SampleRate)).Returns(new AudioSamples(chunks[index].Samples, path, SampleRate));
                files.Add(new StreamingFile(path, chunks[index].RelativeTo, "tbs", MediaType.Audio));
            }

            var intercepted = new List<DateTime>();
            var hashes = new List<AVHashes>();
            await foreach (var entry in FingerprintCommandBuilder.Instance
                .BuildRealtimeFingerprintCommand()
                .From(Cold(files))
                .InterceptAVTrack(track =>
                {
                    intercepted.Add(track.RelativeTo);
                    return track;
                })
                .UsingServices(audioService.Object)
                .Hash(CancellationToken.None))
            {
                hashes.Add(entry);
            }

            Assert.That(hashes, Has.Count.EqualTo(files.Count));
            Assert.Multiple(() =>
            {
                Assert.That(intercepted, Is.EqualTo(files.Select(_ => _.RelativeTo)).AsCollection);
                Assert.That(hashes.Select(_ => _.StreamId), Has.All.EqualTo("tbs"));
                Assert.That(hashes.Select(_ => _.RelativeTo), Is.EqualTo(files.Select(_ => _.RelativeTo)).AsCollection);
            });
        }

        private static DateTime Expected(int index)
        {
            return Start.AddTicks((long)index * MinSamples * TimeSpan.TicksPerSecond / SampleRate);
        }

        private static IEnumerable<int[]> Bins(AVHashes hashes)
        {
            return hashes.Audio!.OrderBy(_ => _.SequenceNumber).Select(_ => _.HashBins);
        }

        private static List<AudioSamples> Chunks(int count, Func<int, DateTime> stampAt)
        {
            return Enumerable.Range(0, count)
                .Select(index => new AudioSamples(TestUtilities.GenerateRandomFloatArray(MinSamples, index + 1), "cnn", SampleRate, stampAt(index)))
                .ToList();
        }

        private static async Task<List<AVHashes>> Hash(
            IAsyncEnumerable<AudioSamples> source,
            Func<AVFingerprintConfiguration, AVFingerprintConfiguration> amend = null,
            Func<AVTrack, AVTrack> avTrackInterceptor = null)
        {
            var result = new List<AVHashes>();
            await foreach (var hashes in FingerprintCommandBuilder.Instance
                .BuildRealtimeFingerprintCommand()
                .From(source)
                .WithFingerprintConfig(amend ?? (config => config))
                .InterceptAVTrack(avTrackInterceptor ?? (track => track))
                .Hash(CancellationToken.None))
            {
                result.Add(hashes);
            }

            return result;
        }

        private static async IAsyncEnumerable<T> Cold<T>(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                await Task.Yield();
                yield return item;
            }
        }
    }
}
