namespace SoundFingerprinting.Tests.Unit.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Utils;

    [TestClass]
    public class TestRunnerUtilsTest
    {
        private readonly TestRunnerUtils testRunnerUtils = new TestRunnerUtils();

        [TestMethod]
        public void ShouldCaptureAllAudioFilesFromFolder()
        {
            string path = Path.GetFullPath("TestEnvironment");

            var files = testRunnerUtils.ListFiles(path, new List<string> { "*.mp3", "*.bin" });

            Assert.AreEqual(2, files.Count);
            var filenames = files.Select(Path.GetFileNameWithoutExtension).ToList();
            Assert.IsTrue(filenames.Contains("Kryptonite"));
            Assert.IsTrue(filenames.Contains("floatsamples"));
        }

        [TestMethod]
        public void ShouldParseInts()
        {
            const string Ints = "1|2|3|4|5";

            var result = testRunnerUtils.ParseInts(Ints, '|');

            CollectionAssert.AreEqual(new List<int> { 1, 2, 3, 4, 5 }, result);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ShouldFailParsingInts()
        {
            const string Ints = "1|2|3|4|%";

            testRunnerUtils.ParseInts(Ints, '|');
        }
    }
}
