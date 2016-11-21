namespace SoundFingerprinting.Windows
{
    /// <summary>
    ///   Hanning window function
    /// </summary>
    internal class HanningWindow : IWindowFunction
    {
        /// <summary>
        ///   Window the outer space by Hanning window function
        /// </summary>
        /// <param name = "outerspace">Array to be windowed</param>
        /// <param name = "length">Window length</param>
        /// <remarks>
        ///   For additional explanation please consult http://en.wikipedia.org/wiki/Hann_function
        /// </remarks>
        public void WindowInPlace(float[] outerspace, int length)
        {
            // Hanning window of the whole signal
            for (int i = 0, n = length; i < n; i++)
            {
                outerspace[i] *= (float)(0.5 * (1 - System.Math.Cos(2 * System.Math.PI * i / (n - 1))));
            }
        }

        /// <summary>
        ///   Gets the corresponding window function
        /// </summary>
        /// <param name = "length">Length of the window</param>
        /// <returns>Window function</returns>
        public float[] GetWindow(int length)
        {
            float[] array = new float[length];

            for (int i = 0; i < length; i++)
            {
                array[i] = (float)(0.5 * (1 - System.Math.Cos(2 * System.Math.PI * i / (length - 1))));
            }

            return array;
        }
    }
}
