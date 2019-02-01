using NUnit.Framework;
using System;
using System.Collections.Generic;
using SoundFingerprinting.Utils;

namespace SoundFingerprinting.Tests.Unit.Utils
{
    [TestFixture]
    public class TypeExtensionMethodsTest
    {
        [Test]
        public void GetNameWithGenericArgs_SupportsNonGenericTypes()
        {
            Assert.AreEqual("Int32", typeof(int).GetNameWithGenericArgs());
            Assert.AreEqual("UInt32", typeof(UInt32).GetNameWithGenericArgs());
            Assert.AreEqual("String", typeof(string).GetNameWithGenericArgs());
        }

        [Test]
        public void GetNameWithGenericArgs_SupportsNullableTypes()
        {
            Assert.AreEqual("Nullable<Int64>", typeof(long?).GetNameWithGenericArgs());
        }

        [Test]
        public void GetNameWithGenericArgs_SupportsGenericTypes()
        {
            Assert.AreEqual("List<String>", typeof(List<string>).GetNameWithGenericArgs());
            Assert.AreEqual("Dictionary<Int32, List<String>>", typeof(Dictionary<int, List<string>>).GetNameWithGenericArgs());
            Assert.AreEqual("Func<List<Dictionary<String, Object>>, Boolean>", typeof(Func<List<Dictionary<string, object>>, bool>).GetNameWithGenericArgs());
        }
    }
}
