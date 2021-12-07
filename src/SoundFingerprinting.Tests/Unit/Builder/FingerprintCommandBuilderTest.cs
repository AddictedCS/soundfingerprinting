namespace SoundFingerprinting.Tests.Unit.Builder
{
    using System.Threading.Tasks;
    using Moq;
    using NUnit.Framework;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Content;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Media;

    [TestFixture]
    public class FingerprintCommandBuilderTest
    {
        [Test]
        public async Task ShouldFingerprintBothAudioAndVideo()
        {
            var mediaService = new Mock<IMediaService>();

            mediaService.Setup(_ => _.ReadAVTrackFromFile("test.mp4", It.IsAny<AVTrackReadConfiguration>(), 0d, 0d, MediaType.Audio | MediaType.Video))
                .Returns(new AVTrack(new AudioTrack(TestUtilities.GenerateRandomAudioSamples(30 * 5512), 30), new VideoTrack(TestUtilities.GenerateRandomFrames(30 * 30), 30)));
            
            var (audio, video)  = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From("test.mp4", MediaType.Audio | MediaType.Video)
                .UsingServices(mediaService.Object)
                .Hash();
            
            Assert.IsNotNull(audio);
            Assert.AreEqual(audio.DurationInSeconds, 30, 0.001);
            
            Assert.IsNotNull(video);
            Assert.AreEqual(video.DurationInSeconds, 30, 0.001);
            
            mediaService.Verify(_ => _.ReadAVTrackFromFile("test.mp4", It.IsAny<AVTrackReadConfiguration>(), 0d, 0d, MediaType.Audio | MediaType.Video), Times.Exactly(1));
        }
    }
}