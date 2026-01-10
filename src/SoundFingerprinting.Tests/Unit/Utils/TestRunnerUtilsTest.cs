namespace SoundFingerprinting.Tests.Unit.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Integration;

    using NUnit.Framework;

    using SoundFingerprinting.Utils;

    [TestFixture]
    public class TestRunnerUtilsTest : IntegrationWithSampleFilesTest
    {
        private readonly TestRunnerUtils testRunnerUtils = new TestRunnerUtils();

        [Test]
        public void ShouldCaptureAllAudioFilesFromFolder()
        {
            string path = TestContext.CurrentContext.TestDirectory;

            var files = testRunnerUtils.ListFiles(path, new List<string> { "*.wav" });

            var fileNames = files.Select(Path.GetFileNameWithoutExtension).ToList();
            var unique = new HashSet<string>(fileNames);
			Assert.That(unique, Has.Count.EqualTo(1));
			Assert.That(unique, Does.Contain(Path.GetFileNameWithoutExtension(PathToWav)));
        }

        [Test]
        public void ShouldParseInts()
        {
            const string ints = "1|2|3|4|5";

            var result = testRunnerUtils.ParseInts(ints, '|');

			Assert.That(result, Is.EqualTo(new List<int> { 1, 2, 3, 4, 5 }).AsCollection);
        }

        [Test]
        public void ShouldFailParsingInts()
        {
            const string ints = "1|2|3|4|%";

            Assert.Throws<FormatException>(() => testRunnerUtils.ParseInts(ints, '|'));
        }
    }
}
