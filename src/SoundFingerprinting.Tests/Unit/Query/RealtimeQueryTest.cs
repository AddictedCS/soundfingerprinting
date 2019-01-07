namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Strides;

    [TestFixture]
    public class RealtimeQueryTest
    {
        [Test]
        public async Task ShouldQueryInRealtime()
        {
            var audioService = new SoundFingerprintingAudioService();
            var modelService = new InMemoryModelService();

            int count = 10, found = 0, didNotPassThreshold = 0, thresholdVotes = 4, testWaitTime = 30000;
            var data = GenerateRandomAudioChunks(count);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance
                                                .BuildFingerprintCommand()
                                                .From(concatenated)
                                                .UsingServices(audioService)
                                                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen", concatenated.Duration), hashes);
            
            var collection = SimulateRealtimeQueryData(data);

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
                }
                , new IncrementalRandomStride(256, 512));

            var cancellationTokenSource = new CancellationTokenSource();
            
            _ = QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                                            .From(collection)
                                            .WithRealtimeQueryConfig(realtimeConfig)
                                            .UsingServices(modelService)
                                            .Query(cancellationTokenSource.Token);

            await Task.Delay(testWaitTime);
            cancellationTokenSource.Cancel();
            
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

        private static BlockingCollection<AudioSamples> SimulateRealtimeQueryData(IReadOnlyCollection<AudioSamples> audioSamples)
        {
            var collection = new BlockingCollection<AudioSamples>();
            Task.Factory.StartNew(async () =>
            {
                await Jitter(collection);
                
                foreach (var audioSample in audioSamples)
                {
                    collection.Add(audioSample);
                    await Task.Delay(TimeSpan.FromSeconds(audioSample.Duration));
                }

                await Jitter(collection);
                
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