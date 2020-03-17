﻿namespace SoundFingerprinting.Tests.Unit.Utils
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
            Assert.AreEqual(1, unique.Count);
            Assert.IsTrue(unique.Contains(Path.GetFileNameWithoutExtension(PathToWav)));
        }

        [Test]
        public void ShouldParseInts()
        {
            const string ints = "1|2|3|4|5";

            var result = testRunnerUtils.ParseInts(ints, '|');

            CollectionAssert.AreEqual(new List<int> { 1, 2, 3, 4, 5 }, result);
        }

        [Test]
        public void ShouldFailParsingInts()
        {
            const string ints = "1|2|3|4|%";

            Assert.Throws<FormatException>(() => testRunnerUtils.ParseInts(ints, '|'));
        }
    }
}
