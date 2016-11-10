namespace SoundFingerprinting.Tests.Unit.Math
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Math;

    [TestClass]
    public class HashConverterTest
    {
        private readonly Random random = new Random((int)DateTime.Now.Ticks);
        private readonly HashConverter hashConverter = new HashConverter();

        [TestMethod]
        public void ShouldConvertBackAndFourthForInts()
        {
            ShouldConvertBackAndFourth(100, 25);
        }

        [TestMethod]
        public void ShouldConvertBackAndFourthForSmallInts()
        {
            ShouldConvertBackAndFourth(100, 50);
        }

        [TestMethod]
        public void ShouldConvertBackAndFourthForLongs()
        {
            ShouldConvertBackAndFourth(160, 20);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowAnException()
        {
            ShouldConvertBackAndFourth(120, 20);
        }

        private void ShouldConvertBackAndFourth(int byteArrayLength, int longArrayLength)
        {
            byte[] expectedBytes = GetRandomArray(byteArrayLength);

            long[] expectedLongs = hashConverter.ToLongs(expectedBytes, longArrayLength);
            byte[] actualBytes = hashConverter.ToBytes(expectedLongs, byteArrayLength);

            for (int i = 0; i < byteArrayLength; ++i)
            {
                Assert.AreEqual(expectedBytes[i], actualBytes[i]);
            }

            long[] actualLongs = hashConverter.ToLongs(actualBytes, longArrayLength);
            for (int i = 0; i < longArrayLength; ++i)
            {
                Assert.AreEqual(expectedLongs[i], actualLongs[i]);
            }
        }

        private byte[] GetRandomArray(int length)
        {
            byte[] array = new byte[length];
            for (int i = 0; i < length; ++i)
            {
                array[i] = (byte)random.Next(256);
            }

            return array;
        }
    }
}
