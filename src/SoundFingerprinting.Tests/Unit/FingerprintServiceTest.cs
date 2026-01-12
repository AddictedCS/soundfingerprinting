namespace SoundFingerprinting.Tests.Unit
{
    using System;
    using System.Linq;

    using NUnit.Framework;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Configuration.Frames;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Strides;

    [TestFixture]
    public class FingerprintServiceTest : AbstractTest
    {
        [Test]
        public void ShouldCreateFingerprintsFromAudioSamples()
        {
            // Arrange - 10 seconds of random audio with a known stride
            const int tenSeconds = 5512 * 10;
            const int stride = 512;
            var samples = TestUtilities.GenerateRandomAudioSamples(tenSeconds);
            var config = new DefaultFingerprintConfiguration
            {
                Stride = new IncrementalStaticStride(stride)
            };

            // Calculate expected fingerprints:
            // First fingerprint requires: SamplesPerFingerprint (8192) + WdftSize (2048) - Overlap (64) = 10176 samples
            // Each subsequent fingerprint requires: stride (512) additional samples
            int minSamplesForFirst = config.SamplesPerFingerprint + config.SpectrogramConfig.WdftSize - config.SpectrogramConfig.Overlap;
            int expectedCount = 1 + (tenSeconds - minSamplesForFirst) / stride;

            // Act
            var (fingerprints, hashes) = FingerprintService.Instance.CreateFingerprintsFromAudioSamples(samples, config);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(hashes, Is.Not.Empty);
                Assert.That(fingerprints.Count(), Is.EqualTo(hashes.Count));
                Assert.That(hashes, Has.Count.EqualTo(expectedCount));
            });
        }

        [Test]
        public void ShouldCreateOneFingerprint()
        {
            var configuration = new DefaultFingerprintConfiguration(){Stride = new IncrementalStaticStride(8192)};
            
            // first fingerprint needs the following minimum number of samples to create one fingerprint.
            // SpectrogramConfig.ImageLength * SpectrogramConfig.Overlap + WDFT size - Overlap.
            int minSize = configuration.SamplesPerFingerprint + configuration.SpectrogramConfig.WdftSize - configuration.SpectrogramConfig.Overlap;
            var audioSamples = new AudioSamples(TestUtilities.GenerateRandomFloatArray(minSize), string.Empty, 5512);
            var (fingerprints, hashes) = FingerprintService.Instance.CreateFingerprintsFromAudioSamples(audioSamples, configuration);
			Assert.Multiple(() =>
			{
				Assert.That(hashes, Has.Count.EqualTo(1));
				Assert.That(fingerprints.Count(), Is.EqualTo(1));
			});

			audioSamples = new AudioSamples(TestUtilities.GenerateRandomFloatArray(minSize + configuration.SamplesPerFingerprint), string.Empty, 5512);
            hashes = FingerprintService.Instance.CreateFingerprintsFromAudioSamples(audioSamples, configuration).Hashes;
			Assert.That(hashes, Has.Count.EqualTo(2));
        }

        [Test]
        public void ShouldSaveOriginalPoints()
        {
            var frames = Enumerable.Range(0, 100)
                .Select(index =>
                {
                    byte[] bytes = new byte[128 * 72 * sizeof(float)];
                    Random.Shared.NextBytes(bytes);
                    float[] frame = new float[128 * 72];
                    Buffer.BlockCopy(bytes, 0, frame, 0, bytes.Length);
                    return new Frame(frame, 128, 72, (float) index / 30, (uint) index);
                })
                .ToList();
            
            var fs = new Frames(frames.Select(frame => new Frame(frame.GetImageRowColsCopy(), frame.Rows, frame.Cols, frame.StartsAt, frame.SequenceNumber)), string.Empty, 30);

            var config = new DefaultFingerprintConfiguration
            {
                FrameNormalizationTransform = new NoFrameNormalization(),
                OriginalPointSaveTransform = frame =>
                {
                    byte[] original = new byte[frame.Length * sizeof(float)];
                    Buffer.BlockCopy(frame.ImageRowCols, 0, original, 0, original.Length);
                    return original;
                }
            };

            var (_, hashes) = FingerprintService.Instance.CreateFingerprintsFromImageFrames(fs, config);

			Assert.That(frames, Has.Count.EqualTo(hashes.Count));
            var originalPoints = hashes
                .OrderBy(_ => _.SequenceNumber)
                .Select(_ => _.OriginalPoint)
                .Select(point =>
                {
                    float[] pt = new float[point.Length / 4];
                    Buffer.BlockCopy(point, 0, pt, 0, point.Length);
                    return pt;
                })
                .ToList();
			Assert.That(originalPoints, Is.EqualTo(frames.Select(_ => _.ImageRowCols).ToList()).AsCollection);
        }

        [Test]
        public void ShouldNotCreateFingerprintsForSilenceWithDefaultConfiguration()
        {
            // Arrange - create pure silence audio samples (all zeros)
            var config = new DefaultFingerprintConfiguration();
            int minSamples = config.SamplesPerFingerprint + config.SpectrogramConfig.WdftSize - config.SpectrogramConfig.Overlap;
            var silence = new AudioSamples(new float[minSamples * 2], string.Empty, 5512);

            // Act
            var (fingerprints, hashes) = FingerprintService.Instance.CreateFingerprintsFromAudioSamples(silence, config);

            // Assert - no fingerprints because silence is filtered out by default
            Assert.Multiple(() =>
            {
                Assert.That(hashes, Is.Empty);
                Assert.That(fingerprints.Count(), Is.EqualTo(0));
            });
        }

        [Test]
        public void ShouldCreateFingerprintsForSilenceWhenTreatSilenceAsSignalIsTrue()
        {
            // Arrange - create pure silence audio samples (all zeros)
            var config = new DefaultFingerprintConfiguration
            {
                TreatSilenceAsSignal = true
            };
            int minSamples = config.SamplesPerFingerprint + config.SpectrogramConfig.WdftSize - config.SpectrogramConfig.Overlap;
            var silence = new AudioSamples(new float[minSamples * 2], string.Empty, 5512);

            // Act
            var (fingerprints, hashes) = FingerprintService.Instance.CreateFingerprintsFromAudioSamples(silence, config);

            // Assert - fingerprints are generated because silence is treated as signal
            Assert.Multiple(() =>
            {
                Assert.That(hashes, Is.Not.Empty);
                Assert.That(fingerprints.Count(), Is.GreaterThan(0));
            });
        }
    }
}
