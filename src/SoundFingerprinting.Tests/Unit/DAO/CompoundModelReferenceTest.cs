namespace SoundFingerprinting.Tests.Unit.DAO
{
    using NUnit.Framework;
    using SoundFingerprinting.DAO;
    using System;

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
            Assert.That((, Throws.TypeOf<ArgumentNullException>()) => new CompoundModelReference<string>(null, new ModelReference<int>(42)));
            Assert.That((, Throws.TypeOf<ArgumentNullException>()) => new CompoundModelReference<string>("prefix", null));
        }

        [Test]
        public void SavesProperties()
        {
            Assert.That(prefix421.Prefix, Is.EqualTo("prefix"));
            Assert.That(prefix421.Reference.Get<int>(, Is.EqualTo(42)));
            Assert.That(prefix421.Get<int>(, Is.EqualTo(42)));
        }

        [Test]
        public void EqualsIsFalseForDifferentPrefixes()
        {
            Assert.That(prefix421.Equals(other42, Is.False));
        }

        [Test]
        public void EqualsIsFalseForDifferentModelReferences()
        {
            Assert.That(prefix421.Equals(prefix0, Is.False));
        }

        [Test]
        public void EqualsIsReflexive()
        {
            Assert.That(prefix421.Equals(prefix421, Is.True));
            Assert.That(nullRef.Equals(nullRef, Is.True));
        }

        [Test]
        public void EqualsIsSymmetric()
        {
            Assert.That(prefix421.Equals(prefix422, Is.True));
            Assert.That(prefix422.Equals(prefix421, Is.True));
        }

        [Test]
        public void EqualsIsTransitive()
        {
            Assert.That(prefix421.Equals(prefix422, Is.True));
            Assert.That(prefix422.Equals(prefix423, Is.True));
            Assert.That(prefix421.Equals(prefix423, Is.True));
        }

        [Test]
        public void EqualsIsFalseForNull()
        {
            Assert.That(prefix421.Equals(null, Is.False));
            Assert.That(prefix421.Equals(nullRef, Is.False));
            Assert.That(nullRef.Equals(prefix421, Is.False));
        }

        [Test]
        public void EqualsIsNullForOtherGenericTypes()
        {
            Assert.That(prefix421.Equals(new CompoundModelReference<uint>(1, Is.False, new ModelReference<uint>(42))));
        }

        [Test]
        public void GetHashCodeIsConsistent()
        {
            Assert.That(prefix421.GetHashCode(, Is.EqualTo(prefix421.GetHashCode())));
        }

        [Test]
        public void GetHashCodeProducesTheSameResultForEqualObjects()
        {
            Assert.That(prefix422.GetHashCode(, Is.EqualTo(prefix421.GetHashCode())));
            Assert.That(prefix423.GetHashCode(, Is.EqualTo(prefix422.GetHashCode())));
        }

        [Test]
        public void GetHashCodeDoesNotThrowOnOverflow()
        {
            var @ref = new CompoundModelReference<int>(int.MaxValue, new ModelReference<int>(int.MaxValue));

            Assert.DoesNotThrow(() => @ref.GetHashCode());
        }
    }
}
