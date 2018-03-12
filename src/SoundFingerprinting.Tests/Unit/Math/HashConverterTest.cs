namespace SoundFingerprinting.Tests.Unit.Math
{
    using System;

    using NUnit.Framework;

    using SoundFingerprinting.Math;

    [TestFixture]
    public class HashConverterTest
    {
        private readonly Random random = new Random((int)DateTime.Now.Ticks);
        private readonly HashConverter hashConverter = new HashConverter();

        [Test]
        public void ShouldConvertBackAndFourthForInts()
        {
            ShouldConvertBackAndFourth(100, 25);
        }

        [Test]
        public void ShouldConvertBackAndFourthForSmallInts()
        {
            ShouldConvertBackAndFourth(100, 50);
        }

        [Test]
        public void ShouldConvertBackAndFourthForLongs()
        {
            ShouldConvertBackAndFourth(160, 20);
        }

        [Test]
        public void ShouldConvertBackAndFourthForNonStandard()
        {
            ShouldConvertBackAndFourth(20, 20);
            ShouldConvertBackAndFourth(60, 20);
            ShouldConvertBackAndFourth(100, 20);
            ShouldConvertBackAndFourth(120, 20);
            ShouldConvertBackAndFourth(140, 20);
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
