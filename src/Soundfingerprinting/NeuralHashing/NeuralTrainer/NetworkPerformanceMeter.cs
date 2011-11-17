// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Threading.Tasks;
using Encog.Engine.Network.Activation;
using Encog.ML.Data;
using Encog.Neural.Data.Basic;
using Encog.Neural.Networks;

namespace Soundfingerprinting.NeuralHashing.NeuralTrainer
{
    public static class NetworkPerformanceMeter
    {
        private static readonly Object LockObject = new object();

        /// <summary>
        ///   Measure the performance of the network
        /// </summary>
        /// <param name = "network">Network to analyze</param>
        /// <param name = "dataset">Dataset with input and ideal data</param>
        /// <returns>Error % of correct bits, returned by the network.</returns>
        public static double MeasurePerformance(BasicNetwork network, BasicNeuralDataSet dataset)
        {
            int correctBits = 0;
            float threshold = 0.0f;
            IActivationFunction activationFunction = network.GetActivation(network.LayerCount - 1); //get the activation function of the output layer
            if (activationFunction is ActivationSigmoid)
            {
                threshold = 0.5f; /* > 0.5, range of sigmoid [0..1]*/
            }
            else if (activationFunction is ActivationTANH)
            {
                threshold = 0.0f; /*> 0, range of bipolar sigmoid is [-1..1]*/
            }
            else
                throw new ArgumentException("Bad activation function");
            int n = (int) dataset.Count;

            Parallel.For(0, n, (i) =>
                               {
                                   IMLData actualOutputs = network.Compute(dataset.Data[i].Input);
                                   lock (LockObject)
                                   {
                                       for (int j = 0, k = actualOutputs.Count; j < k; j++)
                                           if ((actualOutputs[j] > threshold && dataset.Data[i].Ideal[j] > threshold)
                                               || (actualOutputs[j] < threshold && dataset.Data[i].Ideal[j] < threshold))
                                               correctBits++;
                                   }
                               });

            long totalOutputBitsCount = dataset.Count*dataset.Data[0].Ideal.Count;

            return (double) correctBits/totalOutputBitsCount;
        }
    }
}