namespace SoundFingerprinting.Tests.Unit.Wavelets
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Wavelets;

    [TestClass]
    public class WaveletServiceTest : AbstractTest
    {
        private WaveletService waveletService;

        private Mock<IWaveletDecomposition> waveletDecomposition;

        [TestInitialize]
        public void SetUp()
        {
            waveletDecomposition = new Mock<IWaveletDecomposition>(MockBehavior.Strict);

            waveletService = new WaveletService(waveletDecomposition.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            waveletDecomposition.VerifyAll();
        }

        [TestMethod]
        public void DependencyResolverTest()
        {
            var instance = new WaveletService();

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void DecompositionIsInvokedOnEachLogarithmizedFrameTest()
        {
            float[][] firstFrame = new[] { new float[] { 1 } };
            float[][] secondFrame = new[] { new float[] { 2 } };
            float[][] thirdFrame = new[] { new float[] { 3 } };

            waveletDecomposition.Setup(decomposition => decomposition.DecomposeImageInPlace(firstFrame));
            waveletDecomposition.Setup(decomposition => decomposition.DecomposeImageInPlace(secondFrame));
            waveletDecomposition.Setup(decomposition => decomposition.DecomposeImageInPlace(thirdFrame));

            waveletService.ApplyWaveletTransformInPlace(new List<float[][]> { firstFrame, secondFrame, thirdFrame });

            waveletDecomposition.Verify(
                decomposition => decomposition.DecomposeImageInPlace(It.IsAny<float[][]>()), Times.Exactly(3));
        }
    }
}
