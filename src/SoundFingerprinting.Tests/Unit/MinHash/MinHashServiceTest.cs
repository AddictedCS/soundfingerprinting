namespace SoundFingerprinting.Tests.Unit.MinHash
{
    using System;
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

            Assert.Multiple(() =>
            {
                Assert.That(hashed[0], Is.EqualTo(1));
                Assert.That(hashed[1], Is.EqualTo(0));
                Assert.That(hashed[2], Is.EqualTo(255));
            });
        }

        [Test]
        public void ShouldReturnAllMaxValuesForEmptyFingerprint()
        {
            var perms = new TestPermutations();
            var minHashService = new MinHashService(perms);

            // No bits set - all permutations should return 255 (not found)
            byte[] hashed = minHashService.Hash(new TinyFingerprintSchema(10), perms.Count);

            Assert.Multiple(() =>
            {
                Assert.That(hashed[0], Is.EqualTo(255));
                Assert.That(hashed[1], Is.EqualTo(255));
                Assert.That(hashed[2], Is.EqualTo(255));
            });
        }

        [Test]
        public void ShouldFindFirstBitInEachPermutation()
        {
            // Create permutations where each has a known first occurrence
            var perms = new KnownOrderPermutations();
            var minHashService = new MinHashService(perms);

            // Set bits at positions 5, 10, 15
            var fingerprint = new TinyFingerprintSchema(100).SetTrueAt(5, 10, 15);
            byte[] hashed = minHashService.Hash(fingerprint, perms.Count);

            Assert.Multiple(() =>
            {
                // Permutation 0: [5, 10, 15, ...] -> first match at rank 0 (bit 5)
                Assert.That(hashed[0], Is.EqualTo(0));
                // Permutation 1: [10, 5, 15, ...] -> first match at rank 0 (bit 10)
                Assert.That(hashed[1], Is.EqualTo(0));
                // Permutation 2: [0, 1, 2, 5, ...] -> first match at rank 3 (bit 5)
                Assert.That(hashed[2], Is.EqualTo(3));
            });
        }

        [Test]
        public void ShouldProduceSameResultsAsMaxEntropyPermutations()
        {
            var minHashService = MinHashService.MaxEntropy;

            // Create a fingerprint with known bits set across the 8192-bit space
            var fingerprint = new TinyFingerprintSchema(8192);
            var random = new Random(42); // Fixed seed for reproducibility
            for (int i = 0; i < 300; i++)
            {
                fingerprint.SetTrueAt(random.Next(8192));
            }

            byte[] hashed = minHashService.Hash(fingerprint, 100);

            // Verify we got valid results (not all 255s since we set many bits)
            int nonMaxCount = 0;
            for (int i = 0; i < hashed.Length; i++)
            {
                if (hashed[i] < 255)
                {
                    nonMaxCount++;
                }
            }

            // With 300 bits set across 8192 positions, most permutations should find a match
            Assert.That(nonMaxCount, Is.GreaterThan(90), "Expected most permutations to find a matching bit");
        }

        [Test]
        public void ShouldHandleSingleBitSet()
        {
            var perms = new TestPermutations();
            var minHashService = new MinHashService(perms);

            // Set only bit 4
            byte[] hashed = minHashService.Hash(new TinyFingerprintSchema(10).SetTrueAt(4), perms.Count);

            Assert.Multiple(() =>
            {
                // Permutation 0: [1, 4, 8] -> bit 4 is at rank 1
                Assert.That(hashed[0], Is.EqualTo(1));
                // Permutation 1: [2, 3, 8] -> bit 4 not in permutation
                Assert.That(hashed[1], Is.EqualTo(255));
                // Permutation 2: [7, 9, 0] -> bit 4 not in permutation
                Assert.That(hashed[2], Is.EqualTo(255));
            });
        }

        [Test]
        public void ShouldHandleAllBitsSetInPermutationRange()
        {
            var perms = new TestPermutations();
            var minHashService = new MinHashService(perms);

            // Set all bits from 0-9
            var fingerprint = new TinyFingerprintSchema(10);
            for (int i = 0; i < 10; i++)
            {
                fingerprint.SetTrueAt(i);
            }

            byte[] hashed = minHashService.Hash(fingerprint, perms.Count);

            Assert.Multiple(() =>
            {
                // All permutations should find their first bit (rank 0)
                Assert.That(hashed[0], Is.EqualTo(0)); // First bit in perm 0 is 1, which is set
                Assert.That(hashed[1], Is.EqualTo(0)); // First bit in perm 1 is 2, which is set
                Assert.That(hashed[2], Is.EqualTo(0)); // First bit in perm 2 is 7, which is set
            });
        }

        [Test]
        public void ShouldReturnCorrectLengthHash()
        {
            var minHashService = MinHashService.MaxEntropy;
            var fingerprint = new TinyFingerprintSchema(8192).SetTrueAt(100, 200, 300);

            byte[] hashed = minHashService.Hash(fingerprint, 50);

            Assert.That(hashed.Length, Is.EqualTo(50));
        }

        [Test]
        public void ShouldFindMinimumRankWhenMultipleBitsMatch()
        {
            // Permutation: [5, 10, 15] - if bits 10 and 15 are set, should return rank 1 (for bit 10)
            var perms = new SinglePermutation(new[] { 5, 10, 15 });
            var minHashService = new MinHashService(perms);

            var fingerprint = new TinyFingerprintSchema(20).SetTrueAt(10, 15); // bit 5 not set

            byte[] hashed = minHashService.Hash(fingerprint, 1);

            // Bit 10 is at rank 1, bit 15 is at rank 2 -> should return minimum = 1
            Assert.That(hashed[0], Is.EqualTo(1));
        }

        [Test]
        public void ShouldHandleLargeFingerprintWith8192Bits()
        {
            var minHashService = MinHashService.MaxEntropy;

            // Create fingerprint with bits set at various positions across the full range
            var fingerprint = new TinyFingerprintSchema(8192);
            fingerprint.SetTrueAt(0, 100, 1000, 4000, 8000, 8191);

            byte[] hashed = minHashService.Hash(fingerprint, 100);

            // Verify hash array has correct length
            Assert.That(hashed.Length, Is.EqualTo(100));

            // Verify at least some permutations found matches (not all 255)
            bool hasMatch = false;
            for (int i = 0; i < hashed.Length; i++)
            {
                if (hashed[i] < 255)
                {
                    hasMatch = true;
                    break;
                }
            }

            Assert.That(hasMatch, Is.True, "Expected at least one permutation to find a matching bit");
        }

        [Test]
        public void ShouldBeConsistentAcrossMultipleCalls()
        {
            var minHashService = MinHashService.MaxEntropy;
            var fingerprint = new TinyFingerprintSchema(8192).SetTrueAt(42, 100, 500, 1000);

            byte[] hash1 = minHashService.Hash(fingerprint, 100);
            byte[] hash2 = minHashService.Hash(fingerprint, 100);

            Assert.That(hash1, Is.EqualTo(hash2), "Same fingerprint should produce identical hashes");
        }

        private class TestPermutations : IPermutations
        {
            private readonly int[][] perms = [[1, 4, 8], [2, 3, 8], [7, 9, 0]];

            public int[][] GetPermutations()
            {
                return perms;
            }

            public int Count => perms.Length;

            public int IndexesPerPermutation => perms[0].Length;
        }

        private class KnownOrderPermutations : IPermutations
        {
            private readonly int[][] perms =
            [
                [5, 10, 15, 20, 25],      // Bit 5 at rank 0
                [10, 5, 15, 20, 25],      // Bit 10 at rank 0, bit 5 at rank 1
                [0, 1, 2, 5, 10, 15]      // Bit 5 at rank 3
            ];

            public int[][] GetPermutations() => perms;

            public int Count => perms.Length;

            public int IndexesPerPermutation => perms[0].Length;
        }

        private class SinglePermutation(int[] permutation) : IPermutations
        {
            private readonly int[][] perms = [permutation];

            public int[][] GetPermutations() => perms;

            public int Count => 1;

            public int IndexesPerPermutation => perms[0].Length;
        }
    }
}
