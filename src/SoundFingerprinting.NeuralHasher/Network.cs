namespace SoundFingerprinting.NeuralHasher
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    using Encog.Neural.Data.Basic;
    using Encog.Neural.Networks;

    using SoundFingerprinting.Math;

    [Serializable]
    public class Network : BasicNetwork
    {
        public double[] MedianResponces { get; protected set; }

        public static Network Load(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Load(stream);
            }
        }

        public static Network Load(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            Network network = (Network)formatter.Deserialize(stream);
            return network;
        }

        /// <summary>
        ///   Compute median responses of the network
        /// </summary>
        /// <param name = "inputs">Inputs</param>
        /// <param name = "granularity">Number of fingerprints per input</param>
        /// <remarks>
        ///   After propagation for each of the 10 network outputs, if the output was greater than the
        ///   median response of that output (as ascertained from the training set) it was assigned +1, otherwise 0
        /// </remarks>
        public virtual void ComputeMedianResponses(double[][] inputs, int granularity)
        {
            int outputsCount = GetLayerNeuronCount(LayerCount - 1); /*10 - Output length*/
            double[][] responses = new double[outputsCount][];
            int inputsLength = inputs.Length;
            for (int i = 0; i < granularity /*10 Fingerprints*/; i++)
            {
                responses[i] = new double[inputsLength]; /*10240*/
            }

            for (int i = 0; i < inputsLength /*10240*/; i++)
            {
                double[] currentOutputs = Compute(new BasicNeuralData(inputs[i])).Data;
                for (int j = 0; j < granularity /*10*/; j++)
                {
                    responses[j][i] = currentOutputs[j]; /*10240*/
                }
            }

            MedianResponces = new double[outputsCount];

            for (int i = 0; i < outputsCount /*10*/; i++)
            {
                MedianResponces[i] = MathUtility.Median(responses[i]);
            }
        }

        public void Save(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
        }

        public void Save(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Save(stream);
            }
        }
    }
}