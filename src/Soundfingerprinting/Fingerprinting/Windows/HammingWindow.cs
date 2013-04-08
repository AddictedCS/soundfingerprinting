namespace Soundfingerprinting.Fingerprinting.Windows
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Soundfingerprinting.Fingerprinting.FFT;

    public class HammingWindow : IWindowFunction
    {
        /// <summary>
        ///   Perform windowing on a signal in place
        /// </summary>
        /// <param name = "outerspace">Signal to be windowed</param>
        /// <param name = "length">Window length</param>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1407:ArithmeticExpressionsMustDeclarePrecedence", Justification = "Reviewed. Suppression is OK here.")]
        public void WindowInPlace(float[] outerspace, int length)
        {
            for (int i = 0, n = length; i < n; i++)
            {
                outerspace[i] *= (float)(0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (n - 1)));
            }
        }

        /// <summary>
        ///   Perform windowing on a signal in place
        /// </summary>
        /// <param name = "outerspace">Signal to be windowed</param>
        /// <param name = "length">Window length</param>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1407:ArithmeticExpressionsMustDeclarePrecedence", Justification = "Reviewed. Suppression is OK here.")]
        public void WindowInPlace(Complex[] outerspace, int length)
        {
            for (int i = 0, n = length; i < n; i++)
            {
                outerspace[i] *= 0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (n - 1));
            }
        }

        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1407:ArithmeticExpressionsMustDeclarePrecedence", Justification = "Reviewed. Suppression is OK here.")]
        public double[] GetWindow(int length)
        {
            double[] array = new double[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = 0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (length - 1));
            }

            return array;
        }
    }
}