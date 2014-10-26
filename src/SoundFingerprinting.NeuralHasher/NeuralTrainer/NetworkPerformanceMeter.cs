namespace SoundFingerprinting.NeuralHasher.NeuralTrainer
{
    using System;

    using Encog.Engine.Network.Activation;
    using Encog.ML;
    using Encog.Neural.Data.Basic;

    public class NetworkPerformanceMeter
    {
        public double MeasurePerformance(IMLRegression network, BasicNeuralDataSet dataset, IActivationFunction activationFunction)
        {
            int correctBits = 0;
            const float Threshold = 0.0f;
            if (!(activationFunction is ActivationTANH))
            {
                throw new ArgumentException("Bad activation function");
            }

            int n = (int)dataset.Count;

            for (int inputIndex = 0; inputIndex < n; inputIndex++)
            {
                var actualOutputs = network.Compute(dataset.Data[inputIndex].Input);
                for (int outputIndex = 0, k = actualOutputs.Count; outputIndex < k; outputIndex++)
                {
                    if (IsBothPositiveBits(actualOutputs[outputIndex], dataset.Data[inputIndex].Ideal[outputIndex], Threshold)
                        || IsBothNegativeBits(actualOutputs[outputIndex], dataset.Data[inputIndex].Ideal[outputIndex], Threshold))
                    {
                        correctBits++;
                    }
                }
            }

            long totalBitsCount = dataset.Count * dataset.Data[0].Ideal.Count;
            return (double)correctBits / totalBitsCount;
        }

        private bool IsBothNegativeBits(double actualOutput, double ideal, float threshold)
        {
            return actualOutput < threshold && ideal < threshold;
        }

        private bool IsBothPositiveBits(double actualOutput, double ideal, float threshold)
        {
            return actualOutput > threshold && ideal > threshold;
        }
    }
}