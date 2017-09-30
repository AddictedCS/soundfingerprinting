namespace SoundFingerprinting.Tests.Unit.Benchmarks
{
    using System;

    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Running;

    using Moq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;

    [TestFixture]
    public class FingerprintGenerationBenchmark
    {
        private readonly FingerprintCommandBuilder builder = new FingerprintCommandBuilder();
        private readonly Random random = new Random();
        private readonly IAudioService audioService = new Mock<IAudioService>(MockBehavior.Strict).Object;

        [Test]
        public void RunBenchmark()
        {
            var summary = BenchmarkRunner.Run<FingerprintGenerationBenchmark>();
            Console.WriteLine(summary.ToString());
        }

        [Benchmark]
        public void FingerprintGenerationFromRandomAudioSamples()
        {
            var audioSamples = GetAudioSamples(30, 5512);
            var hashedFingerprints = builder.BuildFingerprintCommand()
                .From(new AudioSamples(audioSamples, "temp", 5512))
                .UsingServices(audioService)
                .Hash()
                .Result;
        }

        private float[] GetAudioSamples(int seconds, int sampleRate)
        {
            float[] samples = new float[seconds * sampleRate];
            for (int i = 0; i < samples.Length; ++i)
            {
                samples[i] = (float)this.random.NextDouble();
            }

            return samples;
        }
    }
}
