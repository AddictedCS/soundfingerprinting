namespace SoundFingerprinting.Tests.Unit.Utils
{
    using System.Collections.Generic;
    using System.IO;

    using NUnit.Framework;

    using SoundFingerprinting.Utils;

    [TestFixture]
    public class TestRunnerScenarioValidatorTest
    {
        private readonly TestRunnerScenarioValidator validator = new TestRunnerScenarioValidator();

        [Test]
        public void ShouldNotValidateSinceNoSuchAction()
        {
            string scenario = $"Action,{"C:\\"},IncrementalStatic,0,5115";

            var result = validator.ValidateScenarious(new List<string> { scenario }.ToArray());

            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ShouldValidateInsert()
        {
            string path = TestContext.CurrentContext.TestDirectory;

            string scenario = $"Insert,{path},IncrementalStatic,0,5115";

            var result = validator.ValidateScenarious(new List<string> { scenario }.ToArray());

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ShouldValidateFullRun()
        {
            string path = TestContext.CurrentContext.TestDirectory;

            string scenario1 = $"Insert,{path},IncrementalStatic,0,5115";
            string scenario2 = $"Run,{path},{path},IncrementalRandom,256,512,10,10|30|50";
            string scenario3 = $"Run,{path},{path},IncrementalRandom,512,768,10,10|30|50";
            string scenario4 = $"Run,{path},{path},IncrementalRandom,768,1024,10,10|30|50";
            string scenario5 = $"Insert,{path},IncrementalStatic,0,512";
            string scenario6 = $"Run,{path},{path},IncrementalRandom,256,512,10,10|30|50";

            var result = validator.ValidateScenarious(new List<string> { scenario1, scenario2, scenario3, scenario4, scenario5, scenario6 }.ToArray());

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ShouldValidateRun()
        {
            string path = TestContext.CurrentContext.TestDirectory;

            string scenario = $"Run,{path},{path},IncrementalRandom,256,512,10,10|30|50";

            var result = validator.ValidateScenarious(new List<string> { scenario }.ToArray());

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ShouldNotValidateInsertSinceNoAudioFilesInInputFolder()
        {
            string path = Path.GetTempPath();

            string scenario = $"Insert,{path},IncrementalStatic,0,5115";

            var result = validator.ValidateScenarious(new List<string> { scenario }.ToArray());

            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ShouldNotValidateRunSinceNoAudioFilesInInputFolder()
        {
            string path = Path.GetTempPath();

            string scenario = $"Run,{path},{path},IncrementalRandom,256,512,10,10|30|50";

            var result = validator.ValidateScenarious(new List<string> { scenario }.ToArray());

            Assert.IsFalse(result.IsValid);
        }
    }
}
