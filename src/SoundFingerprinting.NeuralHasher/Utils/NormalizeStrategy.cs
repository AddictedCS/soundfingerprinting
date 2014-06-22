namespace SoundFingerprinting.NeuralHasher.Utils
{
    using System;

    using Encog.Engine.Network.Activation;

    internal class NormalizeStrategy : INormalizeStrategy
    {
        private const double Epsilon = 0.001;
        
        public void NormalizeOutputInPlace(IActivationFunction function, double[] output)
        {
            if (function is ActivationTANH)
            {
                for (int i = 0, n = output.Length; i < n; i++)
                {
                    output[i] = output[i] > 0.5 ? 1 : -1;
                }
            }
            else if (function is ActivationSigmoid)
            {
                /*do nothing*/
            }
            else
            {
                throw new ArgumentException("Unknown activation function");
            }
        }

        public void NormalizeInputInPlace(IActivationFunction function, double[] input)
        {
            if (function is ActivationTANH)
            {
                for (int i = 0, n = input.Length; i < n; i++)
                {
                    input[i] = Math.Abs(input[i] - 0) < Epsilon ? 0.0f : (input[i] < 0 ? -0.8f : 0.8f);
                }
            }
            else if (function is ActivationSigmoid)
            {
                for (int i = 0, n = input.Length; i < n; i++)
                {
                    input[i] = Math.Abs(input[i] - 0) < Epsilon ? 0.0f : (input[i] < 0 ? 0.2f : 0.8f);
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
        }
    }

    internal interface INormalizeStrategy
    {
        void NormalizeOutputInPlace(IActivationFunction function, double[] output);

        void NormalizeInputInPlace(IActivationFunction function, double[] input);
    }
}