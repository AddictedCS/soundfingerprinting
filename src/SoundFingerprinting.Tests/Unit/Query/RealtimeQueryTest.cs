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
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class RealtimeQueryTest
    {
        [Test]
        public async Task ShouldQueryInRealtime()
        {
            var modelService = new InMemoryModelService();

            var data = GenerateRandomAudioSamples("Queen");
            
            var collection = SimulateRealtimeQueryData(data);

            Action<ResultEntry> callback = entry =>
            {
                Console.WriteLine($"Found entry {entry.Track.Title}");
            };

            var realtimeConfig = new RealtimeQueryConfiguration(5, 0.5, callback, TimeSpan.FromSeconds(2));

            var cancellationTokenSource = new CancellationTokenSource();
            
            _ = QueryCommandBuilder.Instance.BuildRealtimeQueryCommand()
                .From(collection)
                .WithRealtimeQueryConfig(realtimeConfig)
                .UsingServices(modelService)
                .Query(cancellationTokenSource.Token);

            await Task.Delay(30000);
        }

        private List<AudioSamples> GenerateRandomAudioSamples(string source)
        {
            var list = new List<AudioSamples>();

            for (int i = 0; i < 10; ++i)
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
                
                collection.CompleteAdding();
            });

            return collection;
        }
    }
}