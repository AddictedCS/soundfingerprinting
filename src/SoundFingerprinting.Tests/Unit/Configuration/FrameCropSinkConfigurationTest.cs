namespace SoundFingerprinting.Tests.Unit.Configuration
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Video;

    [TestFixture]
    public class FrameCropSinkConfigurationTest
    {
        [Test]
        public void TheDefaultConfigurationCropsNothing()
        {
            var configuration = new DefaultAVFingerprintConfiguration();

            Assert.That(configuration.Video.FrameCropSink, Is.Null);
            Assert.That(configuration.GetTrackReadConfiguration().VideoConfig.FrameCropSink, Is.Null);
        }

        [Test]
        public void WithFrameCropSinkShouldCopyTheConfigurationAndLeaveTheOriginalUntouched()
        {
            var shared = new DefaultAVFingerprintConfiguration();
            var sink = new ListSink(new FrameRegion(10, 20, 30, 40, 5));

            var station = shared.WithFrameCropSink(sink);

            Assert.Multiple(() =>
            {
                Assert.That(station, Is.InstanceOf<DefaultAVFingerprintConfiguration>(), "the copy keeps the runtime type");
                Assert.That(station.Video.FrameCropSink, Is.SameAs(sink));
                Assert.That(station.Video.FrameRate, Is.EqualTo(shared.Video.FrameRate));
                Assert.That(station.Video.HashingConfig, Is.SameAs(shared.Video.HashingConfig), "a shallow copy shares the immutable parts");
                Assert.That(station.Audio, Is.SameAs(shared.Audio));
                Assert.That(shared.Video.FrameCropSink, Is.Null, "the shared configuration is not changed");
            });
        }

        [Test]
        public void TheTrackReadConfigurationCarriesTheSink()
        {
            var sink = new ListSink(new FrameRegion(10, 20, 30, 40, 5));
            var configuration = new DefaultAVFingerprintConfiguration().WithFrameCropSink(sink);

            var readConfiguration = configuration.GetTrackReadConfiguration();

            Assert.That(readConfiguration.VideoConfig.FrameCropSink, Is.SameAs(sink));
            Assert.That(readConfiguration.VideoConfig.FrameRate, Is.EqualTo(configuration.Video.FrameRate));
        }

        [TestCase(0, 0, 640, 480, 1, true)]
        [TestCase(600, 400, 40, 80, 1, true)]
        [TestCase(601, 0, 40, 10, 1, false)]
        [TestCase(0, 0, 0, 10, 1, false)]
        [TestCase(-1, 0, 10, 10, 1, false)]
        [TestCase(0, 0, 10, 10, 0, false)]
        public void ARegionIsValidOnlyInsideTheFrameWithAPositiveRate(int x, int y, int width, int height, int fps, bool valid)
        {
            Assert.That(new FrameRegion(x, y, width, height, fps).IsValid, Is.EqualTo(valid));
        }

        private sealed class ListSink : IFrameCropSink
        {
            public ListSink(FrameRegion region)
            {
                Region = region;
            }

            public List<FrameCrop> Crops { get; } = new List<FrameCrop>();

            public FrameRegion? Region { get; }

            public void Add(FrameCrop crop)
            {
                Crops.Add(crop);
            }
        }
    }
}
