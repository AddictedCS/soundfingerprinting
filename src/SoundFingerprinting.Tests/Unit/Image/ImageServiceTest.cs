namespace SoundFingerprinting.Tests.Unit.Image
{
    using System;
    using System.IO;
    using System.Linq;
    using NUnit.Framework;
    using ProtoBuf;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Configuration.Frames;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Image;

    [TestFixture]
    public class ImageServiceTest
    {
        [Test]
        public void ShouldEncodedAndDecodeCorrectly0()
        {
            float[] array = TestUtilities.GenerateRandomFloatArray(128 * 72);
            float[][] image = ImageService.RowCols2Image(array, 72, 128);
            float[] reconverted = ImageService.Image2RowCols(image);

            CollectionAssert.AreEqual(array, reconverted);
        }

        [Test]
        public void ShouldEncodeAndDecodeCorrectly1()
        {
            var image = GetOneImage();
            var encoded = ImageService.Image2RowCols(image);
            var decoded = ImageService.RowCols2Image(encoded, 72, 128);

            for (int i = 0; i < image.Length; ++i)
            {
                CollectionAssert.AreEqual(image[i], decoded[i]);
            }
        }

        [Test]
        public void ShouldEncodeAndDecodeCorrectly2()
        {
            var image = GetOneImage();
            var frame = new Frame(image, 0, 0);
            var decoded = ImageService.RowCols2Image(frame.ImageRowCols, frame.Rows, frame.Cols);

            for (int i = 0; i < image.Length; ++i)
            {
                CollectionAssert.AreEqual(image[i], decoded[i]);
            }
        }
        
        [Test]
        public void ShouldEncodeAndDecodeCorrectly3()
        {
            float[] array = TestUtilities.GenerateRandomFloatArray(128 * 72);
            var frame = new Frame(array, 72, 128, 0, 0);
            var decoded = ImageService.RowCols2Image(frame.ImageRowCols, frame.Rows, frame.Cols);
            var encoded = ImageService.RowCols2Image(array, 72, 128);

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
            var image = GetOneImage();
            
            var frame = new Frame(image, 0, 0);

            var bytes = EncodeFrame(frame);
            var decode = DecodeFrame(bytes);
            
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
            var image = GetOneImage();

            var configuration = new DefaultFingerprintConfiguration
            {
                FrameNormalizationTransform = new NoFrameNormalization(),
                OriginalPointSaveTransform = EncodeFrame
            };

            var (_, hashes) = fingerprintService.CreateFingerprintsFromImageFrames(new Frames(new[] {new Frame(image, 0, 0)}, string.Empty, 30), configuration);

            var decodedFrame = DecodeFrame(hashes.First().OriginalPoint);
            Assert.AreEqual(image.Length, decodedFrame.Length);
            
            for (int i = 0; i < image.Length; ++i)
            {
                CollectionAssert.AreEqual(image[i], decodedFrame[i]);
            }
        }

        private static float[][] DecodeFrame(byte[] originalPoint)
        {
            using var memory = new MemoryStream(originalPoint);
            var frame = Serializer.DeserializeWithLengthPrefix<Frame>(memory, PrefixStyle.Fixed32);
            return ImageService.RowCols2Image(frame.ImageRowCols, frame.Rows, frame.Cols);
        }

        private static byte[] EncodeFrame(Frame frame)
        {
            using var memory = new MemoryStream();
            Serializer.SerializeWithLengthPrefix(memory, frame, PrefixStyle.Fixed32);
            return memory.ToArray();
        }

        private static float[][] GetOneImage()
        {
            var random = new Random(1);
            float[][] image = new float[72][];
            for (int i = 0; i < 72; ++i)
            {
                image[i] = new float[128];
                byte[] buffer = new byte[128 * sizeof(float)];
                random.NextBytes(buffer);
                Buffer.BlockCopy(buffer, 0, image[i], 0, buffer.Length);
            }

            return image;
        }
    }
}