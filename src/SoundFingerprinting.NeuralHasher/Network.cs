namespace SoundFingerprinting.NeuralHasher
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    using Encog.Neural.Data.Basic;
    using Encog.Neural.Networks;

    using SoundFingerprinting.Hashing.Utils;
    using SoundFingerprinting.Math;

    /// <summary>
    ///   Network - represent a collection of connected layers
    /// </summary>
    /// <remarks>
    ///   The network can be saved or loaded, thus serializable
    /// </remarks>
    [Serializable]
    public class Network : BasicNetwork
    {
        public double[] MedianResponces { get; protected set; }

        /// <summary>
        ///   Load network from specified file.
        /// </summary>
        /// <param name = "fileName">File name to load network from.</param>
        /// <returns>Returns instance of <see cref = "Network" /> class with all properties initialized from file.</returns>
        /// <remarks>
        ///   <para>Neural network is loaded from file using .NET serialization (binary formater is used).</para>
        /// </remarks>
        public static Network Load(string fileName)
        {
            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            Network network = Load(stream);
            stream.Close();

            return network;
        }

        /// <summary>
        ///   Load network from specified file.
        /// </summary>
        /// <param name = "stream">Stream to load network from.</param>
        /// <returns>Returns instance of <see cref = "Network" /> class with all properties initialized from file.</returns>
        /// <remarks>
        ///   <para>Neural network is loaded from file using .NET serialization (binary formater is used).</para>
        /// </remarks>
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

        #region I/O Operations

        /// <summary>
        ///   Save network to specified file.
        /// </summary>
        /// <param name = "stream">Stream to save network into.</param>
        /// <remarks>
        ///   <para>The neural network is saved using .NET serialization (binary formatter is used).</para>
        /// </remarks>
        public virtual void Save(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
        }

        public virtual void Save(string fileName)
        {
            FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            Save(stream);
            stream.Close();
        }
        
        #endregion
    }
}