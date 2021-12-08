namespace SoundFingerprinting.Tests.Unit.Content
{
    using System.Linq;
    using NUnit.Framework;
    using SoundFingerprinting.Content;

    [TestFixture]
    public class AudioTrackTest
    {
        [Test]
        public void ShouldSubTrackFullSong()
        {
            var audioTrack = new AudioTrack(TestUtilities.GenerateRandomAudioSamples(30 * 5512));

            var subTracked = audioTrack.SubTrack(0, 0);
            
            Assert.AreEqual(0, subTracked.Duration);
            Assert.IsTrue(subTracked.Samples.Samples.Length == 0);
        }

        [Test]
        public void ShouldSubTrackHalfOfTheTrack()
        {
            var audioSamples = TestUtilities.GenerateRandomAudioSamples(30 * 5512);
            var audioTrack = new AudioTrack(audioSamples);

            var subTracked = audioTrack.SubTrack(0, 15);
            
            Assert.AreEqual(15, subTracked.Duration);
            int half = audioSamples.Samples.Length / 2;
            Assert.AreEqual(half, subTracked.Samples.Samples.Length);
            CollectionAssert.AreEqual(audioSamples.Samples.Take(half), subTracked.Samples.Samples);
        }
    }
}