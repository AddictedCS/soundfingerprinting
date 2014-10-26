namespace SoundFingerprinting.NeuralHasher.Utils
{
    using System;

    using Encog.Engine.Network.Activation;

    internal class NormalizeStrategy : INormalizeStrategy
    {
        public void NormalizeOutputInPlace(IActivationFunction function, double[][] output)
        {
            foreach (var array in output)
            {
                NormalizeOutputInPlace(function, array);
            }
        }

        public void NormalizeOutputInPlace(IActivationFunction function, double[] output)
        {
            if (!(function is ActivationTANH))
            {
                throw new ArgumentException("Only TANH function is supported");
            }
            
            for (int i = 0, n = output.Length; i < n; i++)
            {
                output[i] = output[i] > 0.5 ? 1 : -1;
            }
        }

        public void NormalizeInputInPlace(IActivationFunction function, double[][] input)
        {
            foreach (var array in input)
            {
                NormalizeInputInPlace(function, array);
            }
        }

        public void NormalizeInputInPlace(IActivationFunction function, double[] input)
        {
            if (!(function is ActivationTANH))
            {
                throw new ArgumentException("Only TANH function is supported");
            }
            
            for (int i = 0, n = input.Length; i < n; i++)
            {
                input[i] = input[i] < -1 ? -1 : input[i] > 1 ? 1 : input[i];
            }
        }
    }
}