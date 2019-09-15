namespace SoundFingerprinting.Tests.Unit.Image
{
    using NUnit.Framework;

    using SoundFingerprinting.Image;

    [TestFixture]
    public class ImageServiceTest
    {
        [Test]
        public void ShouldEncodedAndDecodeCorrectly()
        {
            var imageService = new ImageService();

            float[] array = TestUtilities.GenerateRandomFloatArray(128 * 72);
            float[][] image = imageService.RowCols2Image(array, 72, 128);
            float[] reconverted = imageService.Image2RowCols(image);

            CollectionAssert.AreEqual(array, reconverted);
        }
    }
}
