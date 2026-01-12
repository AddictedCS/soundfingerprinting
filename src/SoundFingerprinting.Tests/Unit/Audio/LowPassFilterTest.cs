namespace SoundFingerprinting.Tests.Unit.Audio
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;

    [TestFixture]
    public class LowPassFilterTest
    {
        private const int TargetSampleRate = 5512;

        private readonly LowPassFilter lowPassFilter = new LowPassFilter();

        [TestCase(96000)]
        [TestCase(48000)]
        [TestCase(44100)]
        [TestCase(22050)]
        [TestCase(16000)]
        [TestCase(11025)]
        [TestCase(8000)]
        [TestCase(5512)]
        public void ShouldDownsampleToTargetSampleRate(int sourceSampleRate)
        {
            // Arrange
            int durationSeconds = 10;
            float[] samples = GenerateSineWave(sourceSampleRate, durationSeconds, frequency: 440);

            // Act
            float[] result = lowPassFilter.FilterAndDownsample(samples, sourceSampleRate, TargetSampleRate);

            // Assert
            int expectedLength = sourceSampleRate == TargetSampleRate
                ? samples.Length
                : GetExpectedOutputLength(samples.Length, sourceSampleRate);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
            Assert.That(result.Length, Is.EqualTo(expectedLength).Within(200), 
                $"Unexpected output length for {sourceSampleRate}Hz input");
        }

        [Test]
        public void ShouldReturnSameSamplesWhenSourceEqualsTarget()
        {
            // Arrange
            float[] samples = GenerateSineWave(TargetSampleRate, durationSeconds: 1, frequency: 440);

            // Act
            float[] result = lowPassFilter.FilterAndDownsample(samples, TargetSampleRate, TargetSampleRate);

            // Assert
            Assert.That(result, Is.SameAs(samples));
        }

        [Test]
        public void ShouldThrowForUnsupportedSourceSampleRate()
        {
            // Arrange
            float[] samples = new float[1000];

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                lowPassFilter.FilterAndDownsample(samples, 32000, TargetSampleRate));
        }

        [Test]
        public void ShouldThrowForUnsupportedTargetSampleRate()
        {
            // Arrange
            float[] samples = new float[1000];

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                lowPassFilter.FilterAndDownsample(samples, 44100, 22050));
        }

        [TestCase(48000)]
        [TestCase(44100)]
        public void ShouldAttenuateHighFrequencyContent(int sourceSampleRate)
        {
            // Arrange - create a signal with high frequency content that should be filtered out
            int durationSeconds = 1;
            float[] lowFreqSignal = GenerateSineWave(sourceSampleRate, durationSeconds, frequency: 500);
            float[] highFreqSignal = GenerateSineWave(sourceSampleRate, durationSeconds, frequency: 4000);

            // Act
            float[] lowFreqResult = lowPassFilter.FilterAndDownsample(lowFreqSignal, sourceSampleRate, TargetSampleRate);
            float[] highFreqResult = lowPassFilter.FilterAndDownsample(highFreqSignal, sourceSampleRate, TargetSampleRate);

            // Assert - low frequency signal should retain more energy than high frequency
            double lowFreqEnergy = CalculateEnergy(lowFreqResult);
            double highFreqEnergy = CalculateEnergy(highFreqResult);

            Assert.That(lowFreqEnergy, Is.GreaterThan(highFreqEnergy * 2),
                "Low frequency signal should have significantly more energy after filtering");
        }

        [Test]
        public void ShouldProduceConsistentResultsAcrossMultipleCalls()
        {
            // Arrange
            float[] samples = GenerateSineWave(44100, durationSeconds: 1, frequency: 440);

            // Act
            float[] result1 = lowPassFilter.FilterAndDownsample(samples, 44100, TargetSampleRate);
            float[] result2 = lowPassFilter.FilterAndDownsample(samples, 44100, TargetSampleRate);

            // Assert
            Assert.That(result1, Is.EqualTo(result2));
        }

        [Test]
        public void ShouldHandleShortInputGracefully()
        {
            // Arrange - input shorter than filter length
            float[] samples = new float[50];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = (float)Math.Sin(2 * Math.PI * 440 * i / 44100);
            }

            // Act
            float[] result = lowPassFilter.FilterAndDownsample(samples, 44100, TargetSampleRate);

            // Assert - should produce some output without throwing
            Assert.That(result, Is.Not.Null);
        }

        private static float[] GenerateSineWave(int sampleRate, int durationSeconds, double frequency)
        {
            int sampleCount = sampleRate * durationSeconds;
            float[] samples = new float[sampleCount];
            
            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] = (float)Math.Sin(2 * Math.PI * frequency * i / sampleRate);
            }

            return samples;
        }

        private static double CalculateEnergy(float[] samples)
        {
            return samples.Sum(s => s * s);
        }

        private static int GetExpectedOutputLength(int inputLength, int sourceSampleRate)
        {
            // Calculate expected output length based on resampling ratios
            return sourceSampleRate switch
            {
                96000 => (inputLength / 2 * 7 / 61) - 127, // 96k->48k->5512
                48000 => (inputLength * 7 / 61) - 127,
                44100 => (inputLength / 8) - 31,
                22050 => (inputLength / 4) - 31,
                16000 => (inputLength * 21 / 61) - 127,
                11025 => (inputLength / 2) - 31,
                8000 => (inputLength * 9 / 13) - 63,
                _ => inputLength
            };
        }
    }
}

