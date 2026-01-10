namespace SoundFingerprinting.Tests.Unit.MinHash
{
    using System;
    using NUnit.Framework;

    using SoundFingerprinting.MinHash;
    using SoundFingerprinting.Utils;

    [TestFixture]
    public class ExtendedMinHashServiceTest
    {
        [Test]
        public void ShouldComputeHashCorrectly()
        {
            var perms = new TestPermutations();
            var minHashService = new ExtendedMinHashService(perms);

            int[] hashed = minHashService.Hash(new TinyFingerprintSchema(10).SetTrueAt(2, 4, 6), perms.Count);

            Assert.Multiple(() =>
            {
                Assert.That(hashed[0], Is.EqualTo(1)); // Perm 0: [1, 4, 8] -> bit 4 at rank 1
                Assert.That(hashed[1], Is.EqualTo(0)); // Perm 1: [2, 3, 8] -> bit 2 at rank 0
                Assert.That(hashed[2], Is.EqualTo(3)); // Perm 2: [7, 9, 0] -> no match, returns length 3
            });
        }

        [Test]
        public void ShouldReturnDefaultRankForEmptyFingerprint()
        {
            var perms = new TestPermutations();
            var minHashService = new ExtendedMinHashService(perms);

            // No bits set - all permutations should return the permutation length (not found)
            int[] hashed = minHashService.Hash(new TinyFingerprintSchema(10), perms.Count);

            Assert.Multiple(() =>
            {
                Assert.That(hashed[0], Is.EqualTo(3)); // Length of permutation
                Assert.That(hashed[1], Is.EqualTo(3));
                Assert.That(hashed[2], Is.EqualTo(3));
            });
        }

        [Test]
        public void ShouldFindFirstBitInEachPermutation()
        {
            var perms = new KnownOrderPermutations();
            var minHashService = new ExtendedMinHashService(perms);

            // Set bits at positions 5, 10, 15
            var fingerprint = new TinyFingerprintSchema(100).SetTrueAt(5, 10, 15);
            int[] hashed = minHashService.Hash(fingerprint, perms.Count);

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
        public void ShouldThrowWhenNExceedsPermutationCount()
        {
            var perms = new TestPermutations();
            var minHashService = new ExtendedMinHashService(perms);
            var fingerprint = new TinyFingerprintSchema(10).SetTrueAt(1);

            Assert.Throws<ArgumentException>(() => minHashService.Hash(fingerprint, perms.Count + 1));
        }

        [Test]
        public void ShouldProduceSameResultsWithAdaptivePermutations()
        {
            const int width = 128;
            const int height = 32;
            var perms = new AdaptivePermutations(100, width, height);
            var minHashService = new ExtendedMinHashService(perms);

            // Create a fingerprint with known bits set across the full bit space
            var fingerprint = new TinyFingerprintSchema(width * height * 2);
            var random = new Random(42); // Fixed seed for reproducibility
            for (int i = 0; i < 300; i++)
            {
                fingerprint.SetTrueAt(random.Next(width * height * 2));
            }

            int[] hashed = minHashService.Hash(fingerprint, 100);

            // Verify we got valid results (not all at max since we set many bits)
            int maxRank = perms.IndexesPerPermutation;
            int nonMaxCount = 0;
            for (int i = 0; i < hashed.Length; i++)
            {
                if (hashed[i] < maxRank)
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
            var minHashService = new ExtendedMinHashService(perms);

            // Set only bit 4
            int[] hashed = minHashService.Hash(new TinyFingerprintSchema(10).SetTrueAt(4), perms.Count);

            Assert.Multiple(() =>
            {
                // Permutation 0: [1, 4, 8] -> bit 4 is at rank 1
                Assert.That(hashed[0], Is.EqualTo(1));
                // Permutation 1: [2, 3, 8] -> bit 4 not in permutation, returns length
                Assert.That(hashed[1], Is.EqualTo(3));
                // Permutation 2: [7, 9, 0] -> bit 4 not in permutation, returns length
                Assert.That(hashed[2], Is.EqualTo(3));
            });
        }

        [Test]
        public void ShouldHandleAllBitsSetInPermutationRange()
        {
            var perms = new TestPermutations();
            var minHashService = new ExtendedMinHashService(perms);

            // Set all bits from 0-9
            var fingerprint = new TinyFingerprintSchema(10);
            for (int i = 0; i < 10; i++)
            {
                fingerprint.SetTrueAt(i);
            }

            int[] hashed = minHashService.Hash(fingerprint, perms.Count);

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
            var perms = new AdaptivePermutations(100, 128, 32);
            var minHashService = new ExtendedMinHashService(perms);
            var fingerprint = new TinyFingerprintSchema(8192).SetTrueAt(100, 200, 300);

            int[] hashed = minHashService.Hash(fingerprint, 50);

            Assert.That(hashed.Length, Is.EqualTo(50));
        }

        [Test]
        public void ShouldFindMinimumRankWhenMultipleBitsMatch()
        {
            // Permutation: [5, 10, 15] - if bits 10 and 15 are set, should return rank 1 (for bit 10)
            var perms = new SinglePermutation([5, 10, 15]);
            var minHashService = new ExtendedMinHashService(perms);

            var fingerprint = new TinyFingerprintSchema(20).SetTrueAt(10, 15); // bit 5 not set

            int[] hashed = minHashService.Hash(fingerprint, 1);

            // Bit 10 is at rank 1, bit 15 is at rank 2 -> should return minimum = 1
            Assert.That(hashed[0], Is.EqualTo(1));
        }

        [Test]
        public void ShouldHandleLargeFingerprintWith8192Bits()
        {
            var perms = new AdaptivePermutations(100, 128, 32);
            var minHashService = new ExtendedMinHashService(perms);

            // Create fingerprint with bits set at various positions across the full range
            var fingerprint = new TinyFingerprintSchema(8192);
            fingerprint.SetTrueAt(0, 100, 1000, 4000, 8000, 8191);

            int[] hashed = minHashService.Hash(fingerprint, 100);

            // Verify hash array has correct length
            Assert.That(hashed.Length, Is.EqualTo(100));

            // Verify at least some permutations found matches (not all at max)
            int maxRank = perms.IndexesPerPermutation;
            bool hasMatch = false;
            for (int i = 0; i < hashed.Length; i++)
            {
                if (hashed[i] < maxRank)
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
            var perms = new AdaptivePermutations(100, 128, 32);
            var minHashService = new ExtendedMinHashService(perms);
            var fingerprint = new TinyFingerprintSchema(8192).SetTrueAt(42, 100, 500, 1000);

            int[] hash1 = minHashService.Hash(fingerprint, 100);
            int[] hash2 = minHashService.Hash(fingerprint, 100);

            Assert.That(hash1, Is.EqualTo(hash2), "Same fingerprint should produce identical hashes");
        }

        [Test]
        public void ShouldHandleLargeRanksWithIntReturnType()
        {
            // Create permutation with large number of elements (more than 255)
            const int permLength = 1000;
            var perms = new LargePermutation(permLength);
            var minHashService = new ExtendedMinHashService(perms);

            // Set bit at position 500 (which is at rank 500 in the permutation)
            var fingerprint = new TinyFingerprintSchema(permLength).SetTrueAt(500);

            int[] hashed = minHashService.Hash(fingerprint, 1);

            // Should return rank 500, which is larger than byte max value (255)
            Assert.That(hashed[0], Is.EqualTo(500));
        }

        [Test]
        public void ShouldHandleNoMatchWithLargeDefaultRank()
        {
            const int permLength = 1000;
            var perms = new LargePermutation(permLength);
            var minHashService = new ExtendedMinHashService(perms);

            // No bits set - should return the default rank (permutation length)
            var fingerprint = new TinyFingerprintSchema(permLength);

            int[] hashed = minHashService.Hash(fingerprint, 1);

            Assert.That(hashed[0], Is.EqualTo(permLength));
        }

        private class TestPermutations : IPermutations
        {
            private readonly int[][] perms = [[1, 4, 8], [2, 3, 8], [7, 9, 0]];

            public int[][] GetPermutations() => perms;

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

        private class LargePermutation : IPermutations
        {
            private readonly int[][] perms;

            public LargePermutation(int length)
            {
                // Create a single permutation [0, 1, 2, ..., length-1]
                var perm = new int[length];
                for (int i = 0; i < length; i++)
                {
                    perm[i] = i;
                }

                perms = [perm];
            }

            public int[][] GetPermutations() => perms;

            public int Count => 1;

            public int IndexesPerPermutation => perms[0].Length;
        }
    }
}
