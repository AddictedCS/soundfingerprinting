namespace SoundFingerprinting.Tests.Unit.Content
{
    using System.Linq;
    using NUnit.Framework;
    using SoundFingerprinting.Content;

    [TestFixture]
    public class VideoTrackTest
    {
        [Test]
        public void ShouldSubTrackFullSong()
        {
            var videoTrack = new VideoTrack(TestUtilities.GenerateRandomFrames(30 * 30));

            var subTracked = videoTrack.SubTrack(0, 0);
            
            Assert.That(subTracked.Duration);
        }
        
        [Test]
        public void ShouldSubTrackHalfOfTheTrack()
        {
            var frames = TestUtilities.GenerateRandomFrames(30 * 30);
            var videoTrack = new VideoTrack(frames);

            var subTracked = videoTrack.SubTrack(0, Is.EqualTo(0).Within(15));
            
            Assert.That(subTracked.Duration, Is.EqualTo(15).Within(0.0001));
            int half = frames.Count() / 2;
            Assert.That(subTracked.Frames.Count(, Is.EqualTo(half)));
        }
    }
}