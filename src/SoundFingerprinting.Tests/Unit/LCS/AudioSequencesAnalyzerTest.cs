namespace SoundFingerprinting.Tests.Unit.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.LCS;

    [TestClass]
    public class AudioSequencesAnalyzerTest : AbstractTest
    {
        private readonly IAudioSequencesAnalyzer audioSequencesAnalyzer = new AudioSequencesAnalyzer();

        [TestMethod]
        public void TestStriclyIncreasingSequenceIsAnalyzedCorrectly()
        {
            int[] expected = new[] { 1, 2, 3, 4, 5, 6 };
            var sequence = GetSequence(1, 2, 3, 4, 5, 6);

            var subSequence = audioSequencesAnalyzer.GetLongestIncreasingSubSequence(sequence);

            Assert.AreEqual(sequence.Count, subSequence.Count());
            AssertSequenceAreEqual(expected, sequence);
        }

        [TestMethod]
        public void TestNotStriclyIncreasingSequenceIsAnalyzedCorrectly()
        {
            int[] expected = new[] { 1, 3, 4, 5 };
            int[] order = new[] { 1, 3, 0, 7, 8, 4, 5 };
            var sequence = GetSequence(order);

            var subSequence = audioSequencesAnalyzer.GetLongestIncreasingSubSequence(sequence).ToList();

            Assert.AreEqual(subSequence.Count, 4);
            AssertSequenceAreEqual(expected, subSequence);
        }

        [TestMethod]
        public void TestNotStriclyDecreasingSequenceIsAnalyzedCorrectly()
        {
            int[] expected = new[] { 5 };
            var sequence = GetSequence(5, 4, 3, 2, 1);

            var subSequence = audioSequencesAnalyzer.GetLongestIncreasingSubSequence(sequence).ToList();

            AssertSequenceAreEqual(expected, subSequence);
        }

        [TestMethod]
        public void ShouldReturnMultipleSequencesIfStrongEvidenceOfPresenceOfReccuringSequenceAreFound()
        {
            int[] expected = new[] { 1, 2, 3, 4, 5 };
            var sequence = GetSequence(
                1, 2, 3, 4, 5, 20, 21, 22, 23, 24, 30, 31, 32, 40, 41, 42, 43, 44, 50, 51, 52, 53);

            var subSequences = audioSequencesAnalyzer.GetLongestIncreasingSubSequence(sequence).ToList();

            AssertSequenceAreEqual(expected, subSequences);
        }
        
        private static void AssertSequenceAreEqual(int[] expected, List<SubFingerprintData> subSequence)
        {
            Assert.AreEqual(expected.Length, subSequence.Count);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], subSequence[i].SequenceNumber);
            }
        }

        private static List<SubFingerprintData> GetSequence(params int[] orderNumbers)
        {
            var subfingerprints = orderNumbers.Select(t => new SubFingerprintData(null, t, t * 0.928, null, null)).ToList();
            return subfingerprints;
        }
    }
}
