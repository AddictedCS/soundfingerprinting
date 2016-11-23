namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Moq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Utils;

    [TestFixture]
    [Category("RequiresWindowsDLL")]
    public class TestRunnerTest : IntegrationWithSampleFilesTest
    {
        private readonly IModelService modelService = new InMemoryModelService();
        private readonly IAudioService audioService = new NAudioService();
        private readonly IFingerprintCommandBuilder fcb = new FingerprintCommandBuilder();
        private readonly IQueryCommandBuilder qcb = new QueryCommandBuilder();

        private readonly Mock<NegativeFoundEvent> nfe = new Mock<NegativeFoundEvent>(MockBehavior.Strict);
        private readonly Mock<NegativeNotFoundEvent> nnfe = new Mock<NegativeNotFoundEvent>(MockBehavior.Strict);
        private readonly Mock<PositiveFoundEvent> pfe = new Mock<PositiveFoundEvent>(MockBehavior.Strict);
        private readonly Mock<PositiveNotFoundEvent> pnfe = new Mock<PositiveNotFoundEvent>(MockBehavior.Strict);
        private readonly Mock<TestIterationFinishedEvent> tife = new Mock<TestIterationFinishedEvent>(MockBehavior.Strict);

        private readonly Mock<ITagService> tagService = new Mock<ITagService>(MockBehavior.Strict);

        [SetUp]
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

        [TearDown]
        public void TearDown()
        {
            nfe.VerifyAll();
            nnfe.VerifyAll();
            pfe.VerifyAll();
            pnfe.VerifyAll();
            tife.VerifyAll();
        }

        [Test]
        public void ShouldSuccessfullyRunTest()
        {
            string results = Path.GetTempPath();
            Directory.GetFiles(results).Where(file => file.Contains("results_")).ToList().ForEach(File.Delete);
            Directory.GetFiles(results).Where(file => file.Contains("suite_")).ToList().ForEach(File.Delete);
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

            tife.Setup(e => e(It.IsAny<TestRunner>(), It.IsAny<TestRunnerEventArgs>())).Verifiable();

            string path = Path.GetFullPath(".");
            
            string scenario1 = string.Format("Insert,{0},IncrementalStatic,0,5115", path);
            string scenario2 = string.Format("Run,{0},{1},IncrementalRandom,256,512,10,10|30|50", path, path);
            string scenario3 = string.Format("Insert,{0},IncrementalStatic,0,2048", path);
            string scenario4 = string.Format("Run,{0},{1},IncrementalRandom,256,512,10,10|30|50", path, path);

            var testRunner = new TestRunner(
                new List<string> { scenario1, scenario2, scenario3, scenario4 }.ToArray(),
                modelService,
                audioService,
                tagService.Object,
                fcb,
                qcb,
                results);

            AttachEventHandlers(testRunner);

            testRunner.Run();

            pfe.Verify(e => e(It.IsAny<TestRunner>(), It.IsAny<TestRunnerEventArgs>()), Times.Exactly(6));
            pfe.Verify(e => e(It.IsAny<TestRunner>(), It.IsAny<TestRunnerEventArgs>()), Times.Exactly(6));
            tife.Verify(e => e(It.IsAny<TestRunner>(), It.IsAny<TestRunnerEventArgs>()), Times.Exactly(6));

            var testRuns = Directory.GetFiles(results).Where(file => file.Contains("results_")).ToList();
            Assert.AreEqual(6, testRuns.Count);
            var testSuite = Directory.GetFiles(results).Where(file => file.Contains("suite_")).ToList();
            Assert.AreEqual(1, testSuite.Count);
        }

        private void AttachEventHandlers(TestRunner testRunner)
        {
            testRunner.NegativeFoundEvent += this.nfe.Object;
            testRunner.NegativeNotFoundEvent += this.nnfe.Object;
            testRunner.PositiveFoundEvent += this.pfe.Object;
            testRunner.PositiveNotFoundEvent += this.pnfe.Object;
            testRunner.TestIterationFinishedEvent += this.tife.Object;
        }
    }
}
