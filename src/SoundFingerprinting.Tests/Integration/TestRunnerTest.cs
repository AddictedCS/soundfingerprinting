namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Utils;

    [TestClass]
    public class TestRunnerTest : IntegrationWithSampleFilesTest
    {
        private IModelService modelService = new InMemoryModelService();
        private IAudioService audioService = new NAudioService();
        private IFingerprintCommandBuilder fcb = new FingerprintCommandBuilder();
        private IQueryCommandBuilder qcb = new QueryCommandBuilder();

        private Mock<NegativeFoundEvent> nfe = new Mock<NegativeFoundEvent>(MockBehavior.Strict);
        private Mock<NegativeNotFoundEvent> nnfe = new Mock<NegativeNotFoundEvent>(MockBehavior.Strict);
        private Mock<PositiveFoundEvent> pfe = new Mock<PositiveFoundEvent>(MockBehavior.Strict);
        private Mock<PositiveNotFoundEvent> pnfe = new Mock<PositiveNotFoundEvent>(MockBehavior.Strict);
        private Mock<ITagService> tagService = new Mock<ITagService>(MockBehavior.Strict);


        [TestInitialize]
        public void SetUp()
        {
            tagService.Setup(service => service.GetTagInfo(It.IsAny<string>())).Returns(
                new TagInfo
                    {
                        Artist = "3 Doors Down",
                        Album = "3 Doors Down",
                        Title = "Kryptonite",
                        ISRC = "USUR19980187",
                        Year = 1997,
                        Duration = (3 * 60) + 55
                    });
        }

        [TestCleanup]
        public void TearDown()
        {
            nfe.VerifyAll();
            nnfe.VerifyAll();
            pfe.VerifyAll();
            pnfe.VerifyAll();
        }

        [TestMethod]
        public void ShouldSuccessfullyRunTest()
        {
            pfe.Setup(e => e(It.IsAny<TestRunner>(), It.IsAny<TestRunnerEventArgs>())).Callback(
                (object runner, EventArgs param) =>
                    {
                        var fScore = ((TestRunnerEventArgs)param).FScore;
                        Assert.AreEqual(1, fScore.F1);
                        Assert.AreEqual(1, fScore.Precision);
                        Assert.AreEqual(1, fScore.Recall);
                        Assert.AreEqual(1, ((TestRunnerEventArgs)param).Verified);
                    });

            nfe.Setup(e => e(It.IsAny<TestRunner>(), It.IsAny<TestRunnerEventArgs>())).Callback(
                (object runner, EventArgs param) =>
                    {
                        var fScore = ((TestRunnerEventArgs)param).FScore;
                        Assert.AreEqual(0.6666, fScore.F1, 0.001);
                        Assert.AreEqual(0.5, fScore.Precision, 0.001);
                        Assert.AreEqual(1, fScore.Recall);
                        Assert.AreEqual(2, ((TestRunnerEventArgs)param).Verified);
                    });

            string path = Path.GetFullPath(".");
            
            string scenario1 = string.Format("Insert,{0},IncrementalStatic,0,5115", path);
            string scenario2 = string.Format("Run,{0},{1},IncrementalRandom,256,512,10,10", path, path);
            string results = Path.GetTempPath();

            var testRunner = new TestRunner(
                new List<string> { scenario1, scenario2 }.ToArray(),
                modelService,
                audioService,
                tagService.Object,
                fcb,
                qcb,
                results);

            AttachEventHandlers(testRunner);

            testRunner.Run();
        }

        private void AttachEventHandlers(TestRunner testRunner)
        {
            testRunner.NegativeFoundEvent += this.nfe.Object;
            testRunner.NegativeNotFoundEvent += this.nnfe.Object;
            testRunner.PositiveFoundEvent += this.pfe.Object;
            testRunner.PositiveNotFoundEvent += this.pnfe.Object;
        }
    }
}
