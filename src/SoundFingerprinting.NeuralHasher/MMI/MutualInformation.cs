namespace SoundFingerprinting.NeuralHasher.MMI
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///   Class that allows one to calculate minimal mutual information between 2 samples
    /// </summary>
    public static class MutualInformation
    {
        private const double Epsilon = 0.0001;

        /// <summary>
        ///   Compute minimal mutual information between 2 samples
        /// </summary>
        /// <param name = "samples1">Sample 1</param>
        /// <param name = "samples2">Sample 2</param>
        /// <returns>Value of minimal mutual information</returns>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1407:ArithmeticExpressionsMustDeclarePrecedence", Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public static double Compute(float[] samples1, float[] samples2)
        {
            if (samples1.Length != samples2.Length)
            {
                throw new ArgumentException("The length of arrays should be equal");
            }

            int length = samples1.Length;

            float f00 = 0;
            float f01 = 0;
            float f10 = 0;
            float f11 = 0;
            for (int k = 0; k < length; k++)
            {
                if (samples1[k] < 0.1 && samples2[k] < 0.1)
                {
                    f00++;
                }
                else if (samples1[k] > 0.9 && samples2[k] > 0.9)
                {
                    f11++;
                }
                else if (samples1[k] < 0.1 && samples2[k] > 0.9)
                {
                    f01++;
                }
                else
                {
                    f10++;
                }
            }

            if (Math.Abs(f00 - 0.0) < Epsilon)
            {
                f00++;
            }

            if (Math.Abs(f10 - 0.0) < Epsilon)
            {
                f10++;
            }

            if (Math.Abs(f01 - 0.0) < Epsilon)
            {
                f01++;
            }

            if (Math.Abs(f11 - 0.0) < Epsilon)
            {
                f11++;
            }

            float pX0Y0 = f00 / length;
            float pX0Y1 = f01 / length;
            float pX1Y0 = f10 / length;
            float pX1Y1 = f11 / length;
            float pX0 = pX0Y0 + pX0Y1;
            float pX1 = pX1Y0 + pX1Y1;
            float pY0 = pX0Y0 + pX1Y0;
            float pY1 = pX0Y1 + pX1Y1;

            double mutualInformation =
                (float)
                (pX0Y0 * Math.Log(pX0Y0 / (pX0 * pY0)) + pX0Y1 * Math.Log(pX0Y1 / (pX0 * pY1)) + pX1Y0 * Math.Log(pX1Y0 / (pX1 * pY0))
                 + pX1Y1 * Math.Log(pX1Y1 / (pX1 * pY1)));
            return mutualInformation;
        }
    }
}