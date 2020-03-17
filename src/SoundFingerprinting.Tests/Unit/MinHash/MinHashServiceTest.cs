﻿namespace SoundFingerprinting.Tests.Unit.MinHash
{
    using NUnit.Framework;

    using SoundFingerprinting.MinHash;
    using SoundFingerprinting.Utils;

    [TestFixture]
    public class MinHashServiceTest
    {
        [Test]
        public void ComputeHashTest()
        {
            var perms = new TestPermutations();
            var minHashService = new MinHashService(perms);

            byte[] hashed = minHashService.Hash(new TinyFingerprintSchema(10).SetTrueAt(2, 4, 6), perms.GetPermutations().Length);

            Assert.AreEqual(1, hashed[0]);
            Assert.AreEqual(0, hashed[1]);
            Assert.AreEqual(255, hashed[2]);
        }

        private class TestPermutations : IPermutations
        {
            readonly int[][] perms = { new[] { 1, 4, 8 }, new[] { 2, 3, 8 }, new[] { 7, 9, 0 } };
            public int[][] GetPermutations()
            {
                return perms;
            }

            public int Count => perms.Length;

            public int IndexesPerPermutation => perms[0].Length;
        }
    }
}
