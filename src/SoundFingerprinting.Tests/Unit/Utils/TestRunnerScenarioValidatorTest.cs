﻿namespace SoundFingerprinting.Tests.Unit.Utils
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
            string scenario = string.Format("Action,{0},IncrementalStatic,0,5115", "C:\\");

            var result = validator.ValidateScenarious(new List<string> { scenario }.ToArray());

            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ShouldValidateInsert()
        {
            string path = TestContext.CurrentContext.TestDirectory;

            string scenario = string.Format("Insert,{0},IncrementalStatic,0,5115", path);

            var result = validator.ValidateScenarious(new List<string> { scenario }.ToArray());

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ShouldValidateFullRun()
        {
            string path = TestContext.CurrentContext.TestDirectory;

            string scenario1 = string.Format("Insert,{0},IncrementalStatic,0,5115", path);
            string scenario2 = string.Format("Run,{0},{1},IncrementalRandom,256,512,10,10|30|50", path, path);
            string scenario3 = string.Format("Run,{0},{1},IncrementalRandom,512,768,10,10|30|50", path, path);
            string scenario4 = string.Format("Run,{0},{1},IncrementalRandom,768,1024,10,10|30|50", path, path);
            string scenario5 = string.Format("Insert,{0},IncrementalStatic,0,512", path);
            string scenario6 = string.Format("Run,{0},{1},IncrementalRandom,256,512,10,10|30|50", path, path);

            var result = validator.ValidateScenarious(new List<string> { scenario1, scenario2, scenario3, scenario4, scenario5, scenario6 }.ToArray());

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ShouldValidateRun()
        {
            string path = TestContext.CurrentContext.TestDirectory;

            string scenario = string.Format("Run,{0},{1},IncrementalRandom,256,512,10,10|30|50", path, path);

            var result = validator.ValidateScenarious(new List<string> { scenario }.ToArray());

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ShouldNotValidateInsertSinceNoAudioFilesInInputFolder()
        {
            string path = Path.GetTempPath();

            string scenario = string.Format("Insert,{0},IncrementalStatic,0,5115", path);

            var result = validator.ValidateScenarious(new List<string> { scenario }.ToArray());

            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ShouldNotValidateRunSinceNoAudioFilesInInputFolder()
        {
            string path = Path.GetTempPath();

            string scenario = string.Format("Run,{0},{1},IncrementalRandom,256,512,10,10|30|50", path, path);

            var result = validator.ValidateScenarious(new List<string> { scenario }.ToArray());

            Assert.IsFalse(result.IsValid);
        }
    }
}
