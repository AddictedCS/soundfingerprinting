namespace SoundFingerprinting.NeuralHasher
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    using Encog.Neural.Data.Basic;
    using Encog.Neural.Networks;

    [Serializable]
    public class Network : BasicNetwork
    {
        public double[] MedianResponces { get; protected set; }

        public void ComputeMedianResponses(double[][] inputs, int granularity)
        {
            int outputsCount = GetLayerNeuronCount(LayerCount - 1); // 10 - Output length
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
                MedianResponces[i] = Median(responses[i]);
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

        private double Median(double[] input)
        {
            Array.Sort(input);
            double result;
            int length = input.Length;
            if (length % 2 == 0)
            {
                int middle = length / 2;
                result = (input[middle] + input[middle - 1]) / 2;
            }
            else
            {
                result = input[length / 2];
            }

            return result;
        }
    }
}