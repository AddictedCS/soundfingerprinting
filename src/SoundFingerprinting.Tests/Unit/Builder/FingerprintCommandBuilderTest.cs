namespace SoundFingerprinting.Tests.Unit.Builder
{
    using System;
    using System.Threading.Tasks;
    using Moq;
    using NUnit.Framework;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Content;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Media;
    using SoundFingerprinting.Video;

    [TestFixture]
    public class FingerprintCommandBuilderTest
    {
        [Test]
        public void ShouldThrowWhenMediaServiceIsNotSet()
        {
            Assert.That(async () => await (, Throws.TypeOf<ArgumentException>()) => FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From("test.mp4", MediaType.Audio | MediaType.Video)
                .Hash());
            
            Assert.That(async () => await (, Throws.TypeOf<ArgumentException>()) => FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From("test.mp4", MediaType.Video)
                .UsingServices(new SoundFingerprintingAudioService())
                .Hash());
        }
        
        [Test]
        public async Task ShouldFingerprintBothAudioAndVideo()
        {
            var mediaService = new Mock<IMediaService>();

            mediaService.Setup(_ => _.ReadAVTrackFromFile("test.mp4", It.IsAny<AVTrackReadConfiguration>(), 0d, 0d, MediaType.Audio | MediaType.Video))
                .Returns(new AVTrack(new AudioTrack(TestUtilities.GenerateRandomAudioSamples(30 * 5512)), new VideoTrack(TestUtilities.GenerateRandomFrames(30 * 30))));
            
            var (audio, video)  = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From("test.mp4", MediaType.Audio | MediaType.Video)
                .UsingServices(mediaService.Object)
                .Hash();
            
            Assert.That(audio, Is.Not.Null);
            Assert.That(30, Is.EqualTo(audio.DurationInSeconds).Within(0.001));
            
            Assert.That(video, Is.Not.Null);
            Assert.That(30, Is.EqualTo(video.DurationInSeconds).Within(0.001));
            
            mediaService.Verify(_ => _.ReadAVTrackFromFile("test.mp4", It.IsAny<AVTrackReadConfiguration>(), 0d, 0d, MediaType.Audio | MediaType.Video), Times.Exactly(1));
        }

        [Test]
        public async Task ShouldFingerprintVideoOnly()
        {
            var videoService = new Mock<IVideoService>();

            videoService.Setup(_ => _.ReadFramesFromFile("test.mp4", It.IsAny<VideoTrackReadConfiguration>(), 0d, 0d)).Returns(TestUtilities.GenerateRandomFrames(30 * 30));
            
            var (audio, video)  = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From("test.mp4", MediaType.Video)
                .UsingServices(videoService.Object)
                .Hash();
            
            Assert.That(audio, Is.Null);
            
            Assert.That(video, Is.Not.Null);
            Assert.That(30, Is.EqualTo(video.DurationInSeconds).Within(0.001));
            
            videoService.Verify(_ => _.ReadFramesFromFile("test.mp4", It.IsAny<VideoTrackReadConfiguration>(), 0d, 0d), Times.Exactly(1)); 
        }
    }
}