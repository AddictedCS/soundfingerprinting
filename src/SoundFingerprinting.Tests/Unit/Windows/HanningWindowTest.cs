namespace SoundFingerprinting.Tests.Unit.Windows
{
    using NUnit.Framework;

    using SoundFingerprinting.Windows;

    [TestFixture]
    public class HanningWindowTest
    {
        [Test]
        public void WindowInPlaceTest()
        {
            const int length = 128 * 64;
            float[] outerSpace = TestUtilities.GenerateRandomSingleArray(length);
            float[] outerSpaceCopy = new float[outerSpace.Length];
            outerSpace.CopyTo(outerSpaceCopy, 0);

            var windowFunction = new HanningWindow();
            windowFunction.WindowInPlace(outerSpace, outerSpace.Length);
            WeightByHanningWindow(outerSpaceCopy);

            for (int i = 0; i < outerSpace.Length; i++)
            {
                Assert.AreEqual(true, (outerSpace[i] - outerSpaceCopy[i]) < 0.00001);
            }
        }

        private void WeightByHanningWindow(float[] outerSpace)
        {
            for (int i = 0; i < outerSpace.Length; i++)
            {
                outerSpace[i] *= (float)(0.5 * (1 - System.Math.Cos(System.Math.PI * 2 * i / (outerSpace.Length - 1))));
            }
        }
    }
}
