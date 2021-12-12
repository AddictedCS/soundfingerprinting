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
            
            Assert.AreEqual(0, subTracked.Duration);
        }
        
        [Test]
        public void ShouldSubTrackHalfOfTheTrack()
        {
            var frames = TestUtilities.GenerateRandomFrames(30 * 30);
            var videoTrack = new VideoTrack(frames);

            var subTracked = videoTrack.SubTrack(0, 15);
            
            Assert.AreEqual(15, subTracked.Duration, 0.0001);
            int half = frames.Count() / 2;
            Assert.AreEqual(half, subTracked.Frames.Count());
        }
    }
}