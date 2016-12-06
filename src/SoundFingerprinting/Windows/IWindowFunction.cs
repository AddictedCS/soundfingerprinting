namespace SoundFingerprinting.Windows
{
    internal interface IWindowFunction
    {
        /// <summary>
        ///   Window the outer space by Hanning window function
        /// </summary>
        /// <param name = "outerspace">Array to be windowed</param>
        /// <param name = "length">Window length</param>
        /// <remarks>
        ///   For additional explanation please consult http://en.wikipedia.org/wiki/Hann_function
        /// </remarks>
        void WindowInPlace(float[] outerspace, int length);

        /// <summary>
        ///   Gets the corresponding window function
        /// </summary>
        /// <param name = "length">Length of the window</param>
        /// <returns>Window function</returns>
        float[] GetWindow(int length);
    }
}
