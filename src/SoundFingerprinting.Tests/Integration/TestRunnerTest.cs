namespace SoundFingerprinting.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Moq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Utils;

    [TestFixture]
    public class TestRunnerTest : IntegrationWithSampleFilesTest
    {
        private IModelService modelService;
        private readonly IAudioService audioService = new SoundFingerprintingAudioService();

        private readonly Mock<TestRunnerEvent> nfe = new Mock<TestRunnerEvent>(MockBehavior.Strict);
        private readonly Mock<TestRunnerEvent> nnfe = new Mock<TestRunnerEvent>(MockBehavior.Strict);
        private readonly Mock<TestRunnerEvent> pfe = new Mock<TestRunnerEvent>(MockBehavior.Strict);
        private readonly Mock<TestRunnerEvent> pnfe = new Mock<TestRunnerEvent>(MockBehavior.Strict);
        private readonly Mock<TestRunnerEvent> tife = new Mock<TestRunnerEvent>(MockBehavior.Strict);

        private readonly Mock<ITagService> tagService = new Mock<ITagService>(MockBehavior.Strict);

        [SetUp]
        public void SetUp()
        {
            modelService = new InMemoryModelService();

            tagService.Setup(service => service.GetTagInfo(It.IsAny<string>()))
                      .Returns(new TagInfo
                      {
                        Artist = "Chopin",
                        Album = string.Empty,
                        Title = "Nocturne C#",
                        ISRC = "USUR19980187",
                        Year = 1997,
                        Duration = 193.07d
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
            Directory.GetFiles(results).Where(file => file.Contains("insert_")).ToList().ForEach(File.Delete);
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

            string path = TestContext.CurrentContext.TestDirectory;
            
            string scenario1 = $"Insert,{path},IncrementalStatic,0,5115";
            string scenario2 = $"Run,{path},{path},IncrementalRandom,256,512,7,0|1|2";
            string scenario3 = $"Insert,{path},IncrementalStatic,0,2048";
            string scenario4 = $"Run,{path},{path},IncrementalRandom,256,512,7,0|1|2";

            var testRunner = new TestRunner(
                new List<string> { scenario1, scenario2, scenario3, scenario4 }.ToArray(),
                modelService,
                audioService,
                tagService.Object,
                FingerprintCommandBuilder.Instance,
                QueryCommandBuilder.Instance,
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
            var testInsert = Directory.GetFiles(results).Where(file => file.Contains("insert_")).ToList();
            Assert.AreEqual(2, testInsert.Count);
        }

        private void AttachEventHandlers(TestRunner testRunner)
        {
            testRunner.NegativeFoundEvent += nfe.Object;
            testRunner.NegativeNotFoundEvent += nnfe.Object;
            testRunner.PositiveFoundEvent += pfe.Object;
            testRunner.PositiveNotFoundEvent += pnfe.Object;
            testRunner.TestIterationFinishedEvent += tife.Object;
        }
    }
}
