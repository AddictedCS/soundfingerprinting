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

            int count = 10;
            int found = 0;
            var data = GenerateRandomAudioChunks("Queen", count);
            var concatenated = Concatenate(data);
            var hashes = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
                .From(concatenated)
                .UsingServices(audioService)
                .Hash();

            modelService.Insert(new TrackInfo("312", "Bohemian Rhapsody", "Queen", concatenated.Duration), hashes);
            
            var collection = SimulateRealtimeQueryData(data);

            var realtimeConfig = new RealtimeQueryConfiguration(4, 5,
                entry =>
                {
                    Console.WriteLine($"Found {entry.Track.Title}, Starts At {entry.TrackMatchStartsAt}");
                    Interlocked.Increment(ref found);
                }, 
                TimeSpan.FromSeconds(2), new IncrementalRandomStride(256, 512));

            var cancellationTokenSource = new CancellationTokenSource();
            
            _ = QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                .From(collection)
                .WithRealtimeQueryConfig(realtimeConfig)
                .UsingServices(modelService)
                .Query(cancellationTokenSource.Token);

            await Task.Delay(30000);
            collection.CompleteAdding();
            cancellationTokenSource.Cancel();
            Assert.IsTrue(found >= 3);
        }

        private AudioSamples Concatenate(IReadOnlyList<AudioSamples> data)
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
                Array.Copy(data[i].Samples,0, concatenated, dest, data[i].Samples.Length);
                dest += data[i].Samples.Length;
            }
            
            return new AudioSamples(concatenated, "Queen", 5512);
        }

        private List<AudioSamples> GenerateRandomAudioChunks(string source, int count)
        {
            var list = new List<AudioSamples>();

            for (int i = 0; i < count; ++i)
            {
                var samples = TestUtilities.GenerateRandomFloatArray(10240);
                list.Add(new AudioSamples(samples, source, 5512));
            }

            return list;
        }

        private BlockingCollection<AudioSamples> SimulateRealtimeQueryData(IReadOnlyCollection<AudioSamples> audioSamples)
        {
            var collection = new BlockingCollection<AudioSamples>();
            Task.Factory.StartNew(async () =>
            {
                foreach (var audioSample in audioSamples)
                {
                    collection.Add(audioSample);
                    await Task.Delay((int)audioSample.Duration);
                }
            });

            return collection;
        }
    }
}