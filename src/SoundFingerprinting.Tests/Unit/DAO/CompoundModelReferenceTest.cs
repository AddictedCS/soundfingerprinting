using NUnit.Framework;
using SoundFingerprinting.DAO;
using System;

namespace SoundFingerprinting.Tests.Unit.DAO
{
    [TestFixture]
    public class CompoundModelReferenceTest
    {
        private CompoundModelReference<string> prefix42_1 = new CompoundModelReference<string>("prefix", new ModelReference<int>(42));
        private CompoundModelReference<string> prefix42_2 = new CompoundModelReference<string>("prefix", new ModelReference<int>(42));
        private CompoundModelReference<string> prefix42_3 = new CompoundModelReference<string>("prefix", new ModelReference<int>(42));

        private CompoundModelReference<string> prefix0 = new CompoundModelReference<string>("prefix", new ModelReference<int>(0));
        private CompoundModelReference<string> other42 = new CompoundModelReference<string>("other", new ModelReference<int>(42));
        private CompoundModelReference<string> nullRef = CompoundModelReferenceProvider.Null;

        [Test]
        public void ThrowsForNullArgs()
        {
            Assert.Throws<ArgumentNullException>(() => new CompoundModelReference<string>(null, new ModelReference<int>(42)));
            Assert.Throws<ArgumentNullException>(() => new CompoundModelReference<string>("prefix", null));
        }

        [Test]
        public void SavesProperties()
        {
            Assert.AreEqual("prefix", prefix42_1.Prefix);
            Assert.AreEqual(42, prefix42_1.Reference.Id);
            Assert.AreEqual(42, prefix42_1.Id);
        }

        [Test]
        public void EqualsIsFalseForDifferentPrefixes()
        {
            Assert.IsFalse(prefix42_1.Equals(other42));
        }

        [Test]
        public void EqualsIsFalseForDifferentModelReferences()
        {
            Assert.IsFalse(prefix42_1.Equals(prefix0));
        }

        [Test]
        public void EqualsIsReflexive()
        {
            Assert.IsTrue(prefix42_1.Equals(prefix42_1));
            Assert.IsTrue(nullRef.Equals(nullRef));
        }

        [Test]
        public void EqualsIsSymmetric()
        {
            Assert.IsTrue(prefix42_1.Equals(prefix42_2));
            Assert.IsTrue(prefix42_2.Equals(prefix42_1));
        }

        [Test]
        public void EqualsIsTransitive()
        {
            Assert.IsTrue(prefix42_1.Equals(prefix42_2));
            Assert.IsTrue(prefix42_2.Equals(prefix42_3));
            Assert.IsTrue(prefix42_1.Equals(prefix42_3));
        }

        [Test]
        public void EqualsIsFalseForNull()
        {
            Assert.IsFalse(prefix42_1.Equals(null));
            Assert.IsFalse(prefix42_1.Equals(nullRef));
            Assert.IsFalse(nullRef.Equals(prefix42_1));
        }

        [Test]
        public void EqualsIsNullForOtherGenericTypes()
        {
            Assert.IsFalse(prefix42_1.Equals(new CompoundModelReference<uint>(1, new ModelReference<uint>(42))));
        }

        [Test]
        public void GetHashCodeIsConsistent()
        {
            Assert.AreEqual(prefix42_1.GetHashCode(), prefix42_1.GetHashCode());
        }

        [Test]
        public void GetHashCodeProducesTheSameResultForEqualObjects()
        {
            Assert.AreEqual(prefix42_1.GetHashCode(), prefix42_2.GetHashCode());
            Assert.AreEqual(prefix42_2.GetHashCode(), prefix42_3.GetHashCode());
        }

        [Test]
        public void GetHashCodeDoesNotThrowOnOverflow()
        {
            var @ref = new CompoundModelReference<int>(int.MaxValue, new ModelReference<int>(int.MaxValue));

            Assert.DoesNotThrow(() => @ref.GetHashCode());
        }
    }
}
