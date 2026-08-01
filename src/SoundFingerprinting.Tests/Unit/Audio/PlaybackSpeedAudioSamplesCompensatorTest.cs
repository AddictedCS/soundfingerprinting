namespace SoundFingerprinting.Tests.Unit.Audio
{
    using System;
    using NUnit.Framework;
    using SoundFingerprinting.Audio;

    [TestFixture]
    public class PlaybackSpeedAudioSamplesCompensatorTest
    {
        [Test]
        public void ShouldReturnOriginalSamplesForZeroPlaybackSpeedChange()
        {
            var samples = TestUtilities.GenerateRandomAudioSamples(100);

            var compensated = PlaybackSpeedAudioSamplesCompensator.Compensate(samples, 0);

            Assert.That(compensated, Is.SameAs(samples));
        }

        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(double.NegativeInfinity)]
        [TestCase(double.MaxValue)]
        [TestCase(-100d)]
        [TestCase(-101d)]
        public void ShouldRejectInvalidPlaybackSpeedPercentage(double percentage)
        {
            var samples = TestUtilities.GenerateRandomAudioSamples(100);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => PlaybackSpeedAudioSamplesCompensator.Compensate(samples, percentage));
        }

        [TestCase(8d, 108)]
        [TestCase(-8d, 92)]
        public void ShouldRestoreDurationRepresentedBySourcePlaybackSpeed(double percentage, int expectedLength)
        {
            var samples = TestUtilities.GenerateRandomAudioSamples(100);

            var compensated = PlaybackSpeedAudioSamplesCompensator.Compensate(samples, percentage);

            Assert.Multiple(() =>
            {
                Assert.That(compensated.Samples, Has.Length.EqualTo(expectedLength).Within(1));
                Assert.That(compensated.SampleRate, Is.EqualTo(samples.SampleRate));
                Assert.That(compensated.Origin, Is.EqualTo(samples.Origin));
                Assert.That(compensated.RelativeTo, Is.EqualTo(samples.RelativeTo));
                Assert.That(compensated.TimeOffset, Is.EqualTo(samples.TimeOffset));
            });
        }
    }
}
