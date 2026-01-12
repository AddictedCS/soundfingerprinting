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

			Assert.Multiple(() =>
			{
				Assert.That(subTracked.Duration, Is.EqualTo(0));
				Assert.That(subTracked.Samples.Samples.Length, Is.EqualTo(0));
			});
		}

        [Test]
        public void ShouldSubTrackHalfOfTheTrack()
        {
            var audioSamples = TestUtilities.GenerateRandomAudioSamples(30 * 5512);
            var audioTrack = new AudioTrack(audioSamples);

            var subTracked = audioTrack.SubTrack(0, 15);

			Assert.That(subTracked.Duration, Is.EqualTo(15));
            int half = audioSamples.Samples.Length / 2;
			Assert.That(subTracked.Samples.Samples.Length, Is.EqualTo(half));
			Assert.That(subTracked.Samples.Samples, Is.EqualTo(audioSamples.Samples.Take(half)).AsCollection);
        }
    }
}