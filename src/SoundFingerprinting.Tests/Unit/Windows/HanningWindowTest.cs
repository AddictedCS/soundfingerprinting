namespace SoundFingerprinting.Tests.Unit.Windows
{
    using NUnit.Framework;

    using SoundFingerprinting.Windows;

    [TestFixture]
    public class HanningWindowTest
    {
        private IWindowFunction windowFunction;

        [SetUp]
        public void SetUp()
        {
            windowFunction = new HanningWindow();
        }

        [Test]
        public void WindowInPlaceTest()
        {
            const int Length = 128 * 64;
            float[] outerspace = TestUtilities.GenerateRandomSingleArray(Length);
            float[] outerspaceCopy = new float[outerspace.Length];
            outerspace.CopyTo(outerspaceCopy, 0);

            windowFunction.WindowInPlace(outerspace, outerspace.Length);
            WeightByHanningWindow(outerspaceCopy);

            for (int i = 0; i < outerspace.Length; i++)
            {
                Assert.AreEqual(true, (outerspace[i] - outerspaceCopy[i]) < 0.00001);
            }
        }

        private void WeightByHanningWindow(float[] outerspace)
        {
            for (int i = 0; i < outerspace.Length; i++)
            {
                outerspace[i] *= (float)(0.5 * (1 - System.Math.Cos(System.Math.PI * 2 * i / (outerspace.Length - 1))));
            }
        }
    }
}
