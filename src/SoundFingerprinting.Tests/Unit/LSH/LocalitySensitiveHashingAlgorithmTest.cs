namespace SoundFingerprinting.Tests.Unit.LSH
{
    using System;
    using System.Linq;
    using NUnit.Framework;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.LSH;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.MinHash;
    using SoundFingerprinting.Utils;

    [TestFixture]
    public class LocalitySensitiveHashingAlgorithmTest
    {
        [Test]
        public void FingerprintsCantMatchUniformlyAtRandom()
        {
            var lshAlgorithm = new LocalitySensitiveHashingAlgorithm(new MinHashService(new MaxEntropyPermutations()), new HashConverter());

            var random = new Random();

            var storage = new RAMStorage(25, new IntModelReferenceProvider());
            
            float one = 8192f / 5512;
            var config = new DefaultHashingConfig { NumberOfLSHTables = 25, NumberOfMinHashesPerTable = 4, HashBuckets = 0 };
            
            var track = new ModelReference<int>(1);
            for (int i = 0; i < 1000; ++i)
            {
                int[] trues = Enumerable.Range(0, 200).Select(entry => random.Next(0, 8191)).ToArray();
                var schema = new TinyFingerprintSchema(8192).SetTrueAt(trues);
                var hash = lshAlgorithm.Hash(new Fingerprint(schema, i * one, (uint)i), config, Enumerable.Empty<string>());
                storage.AddHashedFingerprint(hash, track);
            }

            for (int i = 0; i < 100; ++i)
            {
                int[] trues = Enumerable.Range(0, 200).Select(entry => random.Next(0, 8191)).ToArray();
                var schema = new TinyFingerprintSchema(8192).SetTrueAt(trues);
                var hash = lshAlgorithm.Hash(new Fingerprint(schema, i * one, (uint)i), config, Enumerable.Empty<string>());
                for (int j = 0; j < 25; ++j)
                {
                    var ids = storage.GetSubFingerprintsByHashTableAndHash(j, hash.HashBins[j]);
                    Assert.IsFalse(ids.Any());
                }
            }
        }

        [Test]
        public void DistributionOfHashesHasToBeUniform()
        {
            var lshAlgorithm = new LocalitySensitiveHashingAlgorithm(new MinHashService(new MaxEntropyPermutations()), new HashConverter());

            var random = new Random();

            var storage = new RAMStorage(25, new IntModelReferenceProvider());
            
            float one = 8192f / 5512;
            var config = new DefaultHashingConfig { NumberOfLSHTables = 25, NumberOfMinHashesPerTable = 4, HashBuckets = 0 };
            
            var track = new ModelReference<int>(1);
            int l = 100000;
            for (int i = 0; i < l; ++i)
            {
                int[] trues = Enumerable.Range(0, 200).Select(entry => random.Next(0, 8191)).ToArray();
                var schema = new TinyFingerprintSchema(8192).SetTrueAt(trues);
                var hash = lshAlgorithm.Hash(new Fingerprint(schema, i * one, (uint)i), config, Enumerable.Empty<string>());
                storage.AddHashedFingerprint(hash, track);
            }

            var distribution = storage.HashCountsPerTable;

            foreach (var hashPerTable in distribution)
            {
                Assert.IsTrue((double) (l - hashPerTable) / l <= 0.005d, "Less than 0.5 percents of collisions across 100K hashes");
            }
        }
    }
}
