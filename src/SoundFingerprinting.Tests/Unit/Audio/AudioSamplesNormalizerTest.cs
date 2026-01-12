namespace SoundFingerprinting.Tests.Unit.Audio
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;

    [TestFixture]
    public class AudioSamplesNormalizerTest
    {
        private readonly IAudioSamplesNormalizer normalizer = new AudioSamplesNormalizer();

        [Test]
        public void ShouldNormalizeQuietSignal()
        {
            // Arrange - very quiet signal (amplitude 0.01)
            float[] samples = GenerateSineWave(5512, durationSeconds: 1, frequency: 440, amplitude: 0.01f);
            float originalRms = CalculateRms(samples);

            // Act
            normalizer.NormalizeInPlace(samples);

            // Assert - RMS should increase after normalization
            float normalizedRms = CalculateRms(samples);
            Assert.That(normalizedRms, Is.GreaterThan(originalRms), "Quiet signal should be amplified");
        }

        [Test]
        public void ShouldNormalizeLoudSignal()
        {
            // Arrange - loud signal (amplitude 1.0)
            float[] samples = GenerateSineWave(5512, durationSeconds: 1, frequency: 440, amplitude: 1.0f);
            float originalRms = CalculateRms(samples);

            // Act
            normalizer.NormalizeInPlace(samples);

            // Assert - RMS should decrease after normalization
            float normalizedRms = CalculateRms(samples);
            Assert.That(normalizedRms, Is.LessThan(originalRms), "Loud signal should be attenuated");
        }

        [Test]
        public void ShouldClampToValidRange()
        {
            // Arrange - signal that would exceed [-1, 1] after normalization
            float[] samples = GenerateSineWave(5512, durationSeconds: 1, frequency: 440, amplitude: 0.001f);

            // Act
            normalizer.NormalizeInPlace(samples);

            // Assert - all samples should be within [-1, 1]
            Assert.That(samples.All(s => s >= -1f && s <= 1f), Is.True, "All samples should be clamped to [-1, 1]");
        }

        [Test]
        public void ShouldHandleSilence()
        {
            // Arrange - silent signal
            float[] samples = new float[5512];

            // Act
            normalizer.NormalizeInPlace(samples);

            // Assert - should not throw and samples should remain zero or near-zero
            Assert.That(samples.All(s => Math.Abs(s) < 0.001f), Is.True, "Silent signal should remain silent");
        }

        [Test]
        public void ShouldNormalizeWindowedCorrectly()
        {
            // Arrange - signal with varying amplitude in different windows
            int sampleRate = 5512;
            int windowInSeconds = 1;
            int totalSeconds = 3;
            float[] samples = new float[sampleRate * totalSeconds];

            // First second: quiet (amplitude 0.1)
            float[] window1 = GenerateSineWave(sampleRate, 1, 440, 0.1f);
            // Second second: medium (amplitude 0.5)
            float[] window2 = GenerateSineWave(sampleRate, 1, 440, 0.5f);
            // Third second: loud (amplitude 1.0)
            float[] window3 = GenerateSineWave(sampleRate, 1, 440, 1.0f);

            Array.Copy(window1, 0, samples, 0, sampleRate);
            Array.Copy(window2, 0, samples, sampleRate, sampleRate);
            Array.Copy(window3, 0, samples, sampleRate * 2, sampleRate);

            float rms1Before = CalculateRms(samples, 0, sampleRate);
            float rms2Before = CalculateRms(samples, sampleRate, sampleRate);
            float rms3Before = CalculateRms(samples, sampleRate * 2, sampleRate);

            // Act
            normalizer.NormalizeInPlace(samples, sampleRate, windowInSeconds);

            // Assert - each window should be normalized independently
            float rms1After = CalculateRms(samples, 0, sampleRate);
            float rms2After = CalculateRms(samples, sampleRate, sampleRate);
            float rms3After = CalculateRms(samples, sampleRate * 2, sampleRate);

			Assert.Multiple(() =>
			{
				// The quiet window should be amplified
				Assert.That(rms1After, Is.GreaterThan(rms1Before), "Quiet window should be amplified");

				// The loud window should be attenuated
				Assert.That(rms3After, Is.LessThan(rms3Before), "Loud window should be attenuated");
			});

			// After normalization, RMS values should be closer to each other
			float maxRms = Math.Max(rms1After, Math.Max(rms2After, rms3After));
            float minRms = Math.Min(rms1After, Math.Min(rms2After, rms3After));
            float ratio = maxRms / minRms;

            Assert.That(ratio, Is.LessThan(5), "Normalized windows should have more similar RMS values");
        }

        [Test]
        public void ShouldProcessAllWindowsCorrectly()
        {
            // Arrange - multiple windows with same content
            int sampleRate = 5512;
            int windowInSeconds = 1;
            int totalWindows = 5;
            float[] samples = new float[sampleRate * totalWindows];

            // Fill all windows with identical content
            for (int w = 0; w < totalWindows; w++)
            {
                float[] window = GenerateSineWave(sampleRate, 1, 440, 0.5f);
                Array.Copy(window, 0, samples, w * sampleRate, sampleRate);
            }

            // Act
            normalizer.NormalizeInPlace(samples, sampleRate, windowInSeconds);

            // Assert - all windows should have similar RMS after normalization
            float[] rmsValues = new float[totalWindows];
            for (int w = 0; w < totalWindows; w++)
            {
                rmsValues[w] = CalculateRms(samples, w * sampleRate, sampleRate);
            }

            float avgRms = rmsValues.Average();
            Assert.That(rmsValues.All(r => Math.Abs(r - avgRms) < 0.01f), Is.True,
                "All windows with identical input should have identical RMS after normalization");
        }

        [Test]
        public void ShouldHandlePartialLastWindow()
        {
            // Arrange - signal that doesn't divide evenly into windows
            int sampleRate = 5512;
            int windowInSeconds = 1;
            float[] samples = new float[sampleRate * 2 + 100]; // 2 full windows + partial

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = 0.5f * (float)Math.Sin(2 * Math.PI * 440 * i / sampleRate);
            }

            float lastPartRmsBefore = CalculateRms(samples, sampleRate * 2, 100);

            // Act
            normalizer.NormalizeInPlace(samples, sampleRate, windowInSeconds);

            // Assert - partial window should be unchanged (not processed)
            float lastPartRmsAfter = CalculateRms(samples, sampleRate * 2, 100);
            Assert.That(lastPartRmsAfter, Is.EqualTo(lastPartRmsBefore).Within(0.001f),
                "Partial window at the end should not be processed");
        }

        [Test]
        public void ShouldProduceConsistentResults()
        {
            // Arrange
            float[] samples1 = GenerateSineWave(5512, 1, 440, 0.5f);
            float[] samples2 = GenerateSineWave(5512, 1, 440, 0.5f);

            // Act
            normalizer.NormalizeInPlace(samples1);
            normalizer.NormalizeInPlace(samples2);

            // Assert
            Assert.That(samples1, Is.EqualTo(samples2));
        }

        private static float[] GenerateSineWave(int sampleRate, int durationSeconds, double frequency, float amplitude)
        {
            int sampleCount = sampleRate * durationSeconds;
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] = amplitude * (float)Math.Sin(2 * Math.PI * frequency * i / sampleRate);
            }

            return samples;
        }

        private static float CalculateRms(float[] samples)
        {
            return CalculateRms(samples, 0, samples.Length);
        }

        private static float CalculateRms(float[] samples, int start, int length)
        {
            double sum = 0;
            for (int i = start; i < start + length && i < samples.Length; i++)
            {
                sum += samples[i] * samples[i];
            }

            return (float)Math.Sqrt(sum / length);
        }
    }
}

