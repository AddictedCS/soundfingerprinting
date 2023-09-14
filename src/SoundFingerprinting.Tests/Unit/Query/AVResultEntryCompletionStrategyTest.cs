namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using Moq;
    using NUnit.Framework;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class AVResultEntryCompletionStrategyTest
    {
        private Mock<ICompletionStrategy<ResultEntry>> audioStrategy;
        private Mock<ICompletionStrategy<ResultEntry>> videoStrategy;
        private ICompletionStrategy<AVResultEntry> avEntryStrategy;

        [SetUp]
        public void SetUp()
        {
            audioStrategy = new Mock<ICompletionStrategy<ResultEntry>>();
            videoStrategy = new Mock<ICompletionStrategy<ResultEntry>>();
            avEntryStrategy = new AvResultEntryCompletionStrategy(audioStrategy.Object, videoStrategy.Object);
        }

        [Test]
        public void ConstructorThrowsOnInvalidArgs()
        {
            Assert.Throws<ArgumentNullException>(() => new AvResultEntryCompletionStrategy(null, videoStrategy.Object));
            Assert.Throws<ArgumentNullException>(() => new AvResultEntryCompletionStrategy(audioStrategy.Object, null));
        }

        [Test]
        public void NullAVResultEntryCannotContinue()
        {
            Assert.IsFalse(avEntryStrategy.CanContinueInNextQuery(null));
        }

        [Test]
        public void BothAudioAndVideoCanContinue()
        {
            Assert.IsTrue(CanContinue(audio: true, video: true));
        }

        [Test]
        public void OnlyAudioCanContinue()
        {
            Assert.IsTrue(CanContinue(audio: true, video: false));
        }

        [Test]
        public void OnlyVideoCanContinue()
        {
            Assert.IsTrue(CanContinue(audio: false, video: true));
        }

        [Test]
        public void NeitherAudioNorVideoCanContinue()
        {
            Assert.IsFalse(CanContinue(audio: false, video: false));
        }
        
        

        private bool CanContinue(bool audio, bool video)
        {
            var avEntry = new AVResultEntry(CreateResultEntry(), CreateResultEntry());

            audioStrategy.Setup(s => s.CanContinueInNextQuery(avEntry.Audio)).Returns(audio);
            videoStrategy.Setup(s => s.CanContinueInNextQuery(avEntry.Video)).Returns(video);

            return avEntryStrategy.CanContinueInNextQuery(avEntry);
        }

        private static ResultEntry CreateResultEntry()
        {
            var bestPath = new[] { new MatchedWith(0, 0, 0, 0, 0) };
            var coverage = new Coverage(bestPath, 0, 0, 0, 0);
            var track = new TrackData("id", "artist", "title", 100, new ModelReference<uint>(1));
            return new ResultEntry(track, 0, DateTime.Now, coverage);
        }
    }
}