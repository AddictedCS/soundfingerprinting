using NUnit.Framework;
using SoundFingerprinting.DAO;
using System;

namespace SoundFingerprinting.Tests.Unit.DAO
{
    [TestFixture]
    public class CompoundModelReferenceTest
    {
        private readonly CompoundModelReference<string> prefix421 = new CompoundModelReference<string>("prefix", new ModelReference<int>(42));
        private readonly CompoundModelReference<string> prefix422 = new CompoundModelReference<string>("prefix", new ModelReference<int>(42));
        private readonly CompoundModelReference<string> prefix423 = new CompoundModelReference<string>("prefix", new ModelReference<int>(42));

        private readonly CompoundModelReference<string> prefix0 = new CompoundModelReference<string>("prefix", new ModelReference<int>(0));
        private readonly CompoundModelReference<string> other42 = new CompoundModelReference<string>("other", new ModelReference<int>(42));
        private readonly CompoundModelReference<string> nullRef = CompoundModelReferenceProvider.Null;

        [Test]
        public void ThrowsForNullArgs()
        {
            Assert.Throws<ArgumentNullException>(() => new CompoundModelReference<string>(null, new ModelReference<int>(42)));
            Assert.Throws<ArgumentNullException>(() => new CompoundModelReference<string>("prefix", null));
        }

        [Test]
        public void SavesProperties()
        {
            Assert.AreEqual("prefix", prefix421.Prefix);
            Assert.AreEqual(42, prefix421.Reference.Id);
            Assert.AreEqual(42, prefix421.Id);
        }

        [Test]
        public void EqualsIsFalseForDifferentPrefixes()
        {
            Assert.IsFalse(prefix421.Equals(other42));
        }

        [Test]
        public void EqualsIsFalseForDifferentModelReferences()
        {
            Assert.IsFalse(prefix421.Equals(prefix0));
        }

        [Test]
        public void EqualsIsReflexive()
        {
            Assert.IsTrue(prefix421.Equals(prefix421));
            Assert.IsTrue(nullRef.Equals(nullRef));
        }

        [Test]
        public void EqualsIsSymmetric()
        {
            Assert.IsTrue(prefix421.Equals(prefix422));
            Assert.IsTrue(prefix422.Equals(prefix421));
        }

        [Test]
        public void EqualsIsTransitive()
        {
            Assert.IsTrue(prefix421.Equals(prefix422));
            Assert.IsTrue(prefix422.Equals(prefix423));
            Assert.IsTrue(prefix421.Equals(prefix423));
        }

        [Test]
        public void EqualsIsFalseForNull()
        {
            Assert.IsFalse(prefix421.Equals(null));
            Assert.IsFalse(prefix421.Equals(nullRef));
            Assert.IsFalse(nullRef.Equals(prefix421));
        }

        [Test]
        public void EqualsIsNullForOtherGenericTypes()
        {
            Assert.IsFalse(prefix421.Equals(new CompoundModelReference<uint>(1, new ModelReference<uint>(42))));
        }

        [Test]
        public void GetHashCodeIsConsistent()
        {
            Assert.AreEqual(prefix421.GetHashCode(), prefix421.GetHashCode());
        }

        [Test]
        public void GetHashCodeProducesTheSameResultForEqualObjects()
        {
            Assert.AreEqual(prefix421.GetHashCode(), prefix422.GetHashCode());
            Assert.AreEqual(prefix422.GetHashCode(), prefix423.GetHashCode());
        }

        [Test]
        public void GetHashCodeDoesNotThrowOnOverflow()
        {
            var @ref = new CompoundModelReference<int>(int.MaxValue, new ModelReference<int>(int.MaxValue));

            Assert.DoesNotThrow(() => @ref.GetHashCode());
        }
    }
}
