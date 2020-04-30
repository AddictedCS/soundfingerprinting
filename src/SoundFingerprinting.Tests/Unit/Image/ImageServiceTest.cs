namespace SoundFingerprinting.Tests.Unit.Image
{
    using System;
    using System.IO;
    using System.Linq;
    using NUnit.Framework;
    using ProtoBuf;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Image;

    [TestFixture]
    public class ImageServiceTest
    {
        [Test]
        public void ShouldEncodedAndDecodeCorrectly0()
        {
            var imageService = new ImageService();

            float[] array = TestUtilities.GenerateRandomFloatArray(128 * 72);
            float[][] image = imageService.RowCols2Image(array, 72, 128);
            float[] reconverted = imageService.Image2RowCols(image);

            CollectionAssert.AreEqual(array, reconverted);
        }

        [Test]
        public void ShouldEncodeAndDecodeCorrectly1()
        {
            var image = GetOneImage();
            var imageService = new ImageService();
            var encoded = imageService.Image2RowCols(image);
            var decoded = imageService.RowCols2Image(encoded, 72, 128);

            for (int i = 0; i < image.Length; ++i)
            {
                CollectionAssert.AreEqual(image[i], decoded[i]);
            }
        }

        [Test]
        public void ShouldEncodeAndDecodeCorrectly2()
        {
            var image = GetOneImage();
            var imageService = new ImageService();
            var frame = new Frame(image, 0, 0);
            var decoded = imageService.RowCols2Image(frame.ImageRowCols, frame.Rows, frame.Cols);

            for (int i = 0; i < image.Length; ++i)
            {
                CollectionAssert.AreEqual(image[i], decoded[i]);
            }
        }
        
        [Test]
        public void ShouldEncodeAndDecodeCorrectly3()
        {
            float[] array = TestUtilities.GenerateRandomFloatArray(128 * 72);
            var imageService = new ImageService();
            var frame = new Frame(array, 72, 128, 0, 0);
            var decoded = imageService.RowCols2Image(frame.ImageRowCols, frame.Rows, frame.Cols);
            var encoded = imageService.RowCols2Image(array, 72, 128);

            CollectionAssert.AreEqual(array, frame.ImageRowCols);
            
            Assert.AreEqual(decoded.Length, encoded.Length);
            
            for (int i = 0; i < decoded.Length; ++i)
            {
                CollectionAssert.AreEqual(decoded[i], encoded[i]);
            } 
        }
        
        [Test]
        public void ShouldEncodeAndDecodeCorrectly4()
        {
            var imageService = new ImageService();
            var image = GetOneImage();
            
            var frame = new Frame(image, 0, 0);

            var bytes = EncodeFrame(frame);
            var decode = DecodeFrame(bytes, imageService);
            
            Assert.AreEqual(image.Length, decode.Length);
            for (int i = 0; i < image.Length; ++i)
            {
                CollectionAssert.AreEqual(image[i], decode[i]);
            }
        }

        [Test]
        public void ShouldSerializeDeserializeSameArray()
        {
            var fingerprintService = FingerprintService.Instance;
            var imageService = new ImageService();

            var image = GetOneImage();

            var hashes = fingerprintService.CreateFingerprintsFromImageFrames(new Frames(new[] {new Frame(image, 0, 0)}, DateTime.Now, string.Empty), new DefaultFingerprintConfiguration
            {
                OriginalPointSaveTransform = EncodeFrame
            });

            var decodedFrame = DecodeFrame(hashes.First().OriginalPoint, imageService);
            Assert.AreEqual(image.Length, decodedFrame.Length);
            
            for (int i = 0; i < image.Length; ++i)
            {
                CollectionAssert.AreEqual(image[i], decodedFrame[i]);
            }
        }

        private static float[][] DecodeFrame(byte[] originalPoint, IImageService imageService)
        {
            using (var memory = new MemoryStream(originalPoint))
            {
                var frame = Serializer.DeserializeWithLengthPrefix<Frame>(memory, PrefixStyle.Fixed32);
                return imageService.RowCols2Image(frame.ImageRowCols, frame.Rows, frame.Cols);
            }
        }

        private static byte[] EncodeFrame(Frame frame)
        {
            using (var memory = new MemoryStream())
            {
                Serializer.SerializeWithLengthPrefix(memory, frame, PrefixStyle.Fixed32);
                return memory.ToArray();
            }
        }

        private static float[][] GetOneImage()
        {
            var random = new Random(1);
            float[][] image = new float[72][];
            for (int i = 0; i < 72; ++i)
            {
                image[i] = new float[128];
                for (int j = 0; j < 128; ++j)
                {
                    image[i][j] = (float) random.NextDouble();
                }
            }

            return image;
        }
    }
}