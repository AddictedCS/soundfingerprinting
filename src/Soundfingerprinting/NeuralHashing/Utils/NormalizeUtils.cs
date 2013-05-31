namespace Soundfingerprinting.NeuralHashing.Utils
{
    using System;

    using Encog.Engine.Network.Activation;

    /// <summary>
    ///   Class for output/input normalization
    /// </summary>
    public static class NormalizeUtils
    {
        /// <summary>
        ///   Normalize in place desired output
        /// </summary>
        /// <param name = "function">Activation function used</param>
        /// <param name = "output">Normalize output</param>
        /// <returns>Normalized output</returns>
        public static double[] NormalizeOneDesiredOutputInPlace(IActivationFunction function, double[] output)
        {
            if (function is ActivationSigmoid)
            {
                for (int i = 0, n = output.Length; i < n; i++)
                {
                    output[i] = (output[i] > 0 ? 0.8 : 0.2);
                }
            }
            else if (function is ActivationTANH)
            {
                for (int i = 0, n = output.Length; i < n; i++)
                {
                    output[i] = (output[i] > 0.5 ? 0.5 : -0.5);
                }
            }
            else if (function is ActivationLinear)
            {
                /*do nothing*/
            }
            else
            {
                throw new ArgumentException("Unknown activation function");
            }

            return output;
        }

        /// <summary>
        ///   Normalize in place desired input
        /// </summary>
        /// <param name = "function">Activation function</param>
        /// <param name = "input">Input to normalize</param>
        /// <returns>Reference to normalized input</returns>
        public static double[] NormalizeDesiredInputInPlace(IActivationFunction function, double[] input)
        {
            if (function is ActivationTANH)
            {
                for (int i = 0, n = input.Length; i < n; i++)
                {
                    input[i] = (input[i] == 0 ? 0.0f : (input[i] < 0 ? -0.8f : 0.8f));
                }
            }
            else if (function is ActivationSigmoid)
            {
                for (int i = 0, n = input.Length; i < n; i++)
                {
                    input[i] = (input[i] == 0 ? 0.0f : (input[i] < 0 ? 0.2f : 0.8f));
                }
            }
            else if (function is ActivationLinear)
            {
                /*do nothing*/
            }
            else
            {
                throw new ArgumentException("Unknown activation function");
            }
            return input;
        }
    }
}