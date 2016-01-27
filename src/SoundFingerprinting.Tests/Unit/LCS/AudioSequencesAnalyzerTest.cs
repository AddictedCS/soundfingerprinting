namespace SoundFingerprinting.Tests.Unit.LCS
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.DAO;
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

            var subSequence = audioSequencesAnalyzer.SortCandiatesByLongestIncresingAudioSequence(sequence, 0.928 * 6).ToList();

            Assert.AreEqual(1, subSequence.Count);
            Assert.AreEqual(sequence.Values.First().Count, subSequence.First().Count());
            AssertSequenceAreEqual(expected, subSequence.First().ToList());
        }

        [TestMethod]
        public void TestNotStriclyIncreasingSequenceIsAnalyzedCorrectly()
        {
            int[] expected = new[] { 3, 4, 5 };
            int[] order = new[] { 1, 3, 7, 8, 4, 5 };
            var sequence = GetSequence(order);

            var subSequence = audioSequencesAnalyzer.SortCandiatesByLongestIncresingAudioSequence(sequence, 1.48).First().ToList();

            Assert.AreEqual(3, subSequence.Count);
            AssertSequenceAreEqual(expected, subSequence);
        }

        [TestMethod]
        public void ShouldReturnMultipleSequencesIfStrongEvidenceOfPresenceOfReccuringSequenceAreFound()
        {
            int[] expected = new[] { 40, 41, 42, 43, 44, 50, 51, 52, 53 };
            var sequence = GetSequence(
                1, 2, 3, 4, 5, 20, 21, 22, 23, 24, 30, 31, 32, 40, 41, 42, 43, 44, 50, 51, 52, 53);

            var subSequences = audioSequencesAnalyzer.SortCandiatesByLongestIncresingAudioSequence(sequence, 5 * 1.48).First().ToList();

            AssertSequenceAreEqual(expected, subSequences);
        }
        
        [TestMethod]
        public void ShouldReturEmptyCollectionInCaseEmptyListIsReceivedAsInput()
        {
            var list = audioSequencesAnalyzer.SortCandiatesByLongestIncresingAudioSequence(new Dictionary<IModelReference, SubfingerprintSetSortedByTimePosition>(), 0);

            Assert.AreEqual(0, list.Count());
        }

        [TestMethod]
        public void ShouldReturnSingleElementInCaseIfSingleElementIsPassedInCollection()
        {
            int[] expected = new[] { 1 };
            var sequence = GetSequence(1);

            var subSequences = audioSequencesAnalyzer.SortCandiatesByLongestIncresingAudioSequence(sequence, 1.48).First().ToList();

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

        private static Dictionary<IModelReference, SubfingerprintSetSortedByTimePosition> GetSequence(params int[] orderNumbers)
        {
            var subfingerprints = orderNumbers.Select(t => new SubFingerprintData(null, t, t * 0.928, null, null)).ToList();
            var candidate = new SubfingerprintSetSortedByTimePosition();
            foreach (var subFingerprintData in subfingerprints)
            {
                candidate.Add(subFingerprintData);
            }

            var track = new ModelReference<int>(123);
            var allCandidates = new Dictionary<IModelReference, SubfingerprintSetSortedByTimePosition>
                {
                    { track, candidate } 
                };
            return allCandidates;
        }
    }
}
