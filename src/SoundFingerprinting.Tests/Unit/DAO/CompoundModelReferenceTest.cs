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
            Assert.Throws<ArgumentNullException>(() => new CompoundModelReference<string>(null, new ModelReference<int>(42)));
            Assert.Throws<ArgumentNullException>(() => new CompoundModelReference<string>("prefix", null));
        }

        [Test]
        public void SavesProperties()
        {
			Assert.Multiple(() =>
			{
				Assert.That(prefix421.Prefix, Is.EqualTo("prefix"));
				Assert.That(prefix421.Reference.Get<int>(), Is.EqualTo(42));
				Assert.That(prefix421.Get<int>(), Is.EqualTo(42));
			});
		}

        [Test]
        public void EqualsIsFalseForDifferentPrefixes()
        {
			Assert.That(prefix421.Equals(other42), Is.False);
        }

        [Test]
        public void EqualsIsFalseForDifferentModelReferences()
        {
			Assert.That(prefix421.Equals(prefix0), Is.False);
        }

        [Test]
        public void EqualsIsReflexive()
        {
			Assert.Multiple(() =>
			{
				Assert.That(prefix421, Is.EqualTo(prefix421));
				Assert.That(nullRef, Is.EqualTo(nullRef));
			});
		}

        [Test]
        public void EqualsIsSymmetric()
        {
			Assert.Multiple(() =>
			{
				Assert.That(prefix421, Is.EqualTo(prefix422));
				Assert.That(prefix422, Is.EqualTo(prefix421));
			});
		}

        [Test]
        public void EqualsIsTransitive()
        {
			Assert.Multiple(() =>
			{
				Assert.That(prefix421, Is.EqualTo(prefix422));
				Assert.That(prefix422, Is.EqualTo(prefix423));
			});
			Assert.That(prefix421, Is.EqualTo(prefix423));
        }

        [Test]
        public void EqualsIsFalseForNull()
        {
			Assert.Multiple(() =>
			{
				Assert.That(prefix421.Equals(null), Is.False);
				Assert.That(prefix421.Equals(nullRef), Is.False);
				Assert.That(nullRef.Equals(prefix421), Is.False);
			});
		}

        [Test]
        public void EqualsIsNullForOtherGenericTypes()
        {
			Assert.That(prefix421.Equals(new CompoundModelReference<uint>(1, new ModelReference<uint>(42))), Is.False);
        }

        [Test]
        public void GetHashCodeIsConsistent()
        {
			Assert.That(prefix421.GetHashCode(), Is.EqualTo(prefix421.GetHashCode()));
        }

        [Test]
        public void GetHashCodeProducesTheSameResultForEqualObjects()
        {
			Assert.Multiple(() =>
			{
				Assert.That(prefix422.GetHashCode(), Is.EqualTo(prefix421.GetHashCode()));
				Assert.That(prefix423.GetHashCode(), Is.EqualTo(prefix422.GetHashCode()));
			});
		}

        [Test]
        public void GetHashCodeDoesNotThrowOnOverflow()
        {
            var @ref = new CompoundModelReference<int>(int.MaxValue, new ModelReference<int>(int.MaxValue));

            Assert.That(() => @ref.GetHashCode(), Throws.Nothing);
        }
    }
}
