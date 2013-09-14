namespace SoundFingerprinting.Hashing.NeuralHashing.Ensemble
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;

    using Encog.Neural.Data.Basic;

    using SoundFingerprinting.Hashing.NeuralHashing.MMI;

    /// <summary>
    ///   Neural network ensemble used in hashing the fingerprints
    /// </summary>
    [Serializable]
    public class NeuralNetworkEnsemble
    {
        /// <summary>
        ///   Inputs count
        /// </summary>
        private readonly int inputsCount;

        /// <summary>
        ///   All networks used in computation
        /// </summary>
        private readonly Network[] networks;

        /// <summary>
        ///   Output vector
        /// </summary>
        private readonly byte[] outputVector;

        /// <summary>
        ///   Total outputs
        /// </summary>
        private readonly int totalOutputs;

        /// <summary>
        ///   Hash pattern
        /// </summary>
        private MinimalMutualInfoPattern hashPattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuralNetworkEnsemble"/> class. 
        ///   Constructor
        /// </summary>
        /// <param name="networks">
        /// An array of trained networks that will work as hash functions
        /// </param>
        /// <exception cref="NeuralNetworkHashingException">
        /// Can not create a NN Ensemble out of no networks
        /// </exception>
        public NeuralNetworkEnsemble(Network[] networks)
        {
            if (networks == null || networks.Length == 0)
            {
                throw new NeuralNetworkHashingException("Can not create a NN Ensemble out of no networks");
            }

            inputsCount = networks[0].InputCount;

            // Check all inputs
            foreach (Network network in networks)
            {
                if (network.InputCount != inputsCount)
                {
                    throw new NeuralNetworkHashingException("All networks should have the same inputs count");
                }

                if (network.MedianResponces == null)
                {
                    throw new NeuralNetworkHashingException(
                        "Median responses of one of the networks is null," + " please run Median Responses computation for each of the networks and try again");
                }
            }

            this.networks = networks;

            foreach (Network network in this.networks)
            {
                totalOutputs += network.GetLayerNeuronCount(network.LayerCount - 1);
            }

            outputVector = new byte[totalOutputs];
        }

        public byte[] OutputVector
        {
            get { return outputVector; }
        }

        public MinimalMutualInfoPattern HashPattern
        {
            set
            {
                if (value.NeuralNetworkEnsembleDimensionality != networks[0].OutputCount * networks.Length)
                {
                    throw new ArgumentException("The hash patter does not correspond to networks of the NeuralNetworkEnsemble");
                }

                hashPattern = value;
            }
        }

        /// <summary>
        ///   Load network ensemble from specified file.
        /// </summary>
        /// <param name = "fileName">File name to load network ensemble from.</param>
        /// <returns>Returns instance of <see cref = "NeuralNetworkEnsemble" /> class with all properties initialized from file.</returns>
        /// <remarks>
        ///   <para>Neural network ensemble is loaded from file using .NET serialization (binary formater is used).</para>
        /// </remarks>
        public static NeuralNetworkEnsemble Load(string fileName)
        {
            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            NeuralNetworkEnsemble network = Load(stream);
            stream.Close();

            return network;
        }

        /// <summary>
        ///   Load network from specified file.
        /// </summary>
        /// <param name = "stream">Stream to load network from.</param>
        /// <returns>Returns instance of <see cref = "NeuralNetworkEnsemble" /> class with all properties initialized from file.</returns>
        /// <remarks>
        ///   <para>Neural network is loaded from file using .NET serialization (binary formatter is used).</para>
        /// </remarks>
        public static NeuralNetworkEnsemble Load(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            NeuralNetworkEnsemble network = (NeuralNetworkEnsemble)formatter.Deserialize(stream);
            return network;
        }

        /// <summary>
        ///   Propagates the input vector through all of the networks of the ensemble
        /// </summary>
        /// <param name = "input">Input vector</param>
        /// <returns>Hashed vector</returns>
        /// <exception cref = "NeuralNetworkHashingException">Input vector should be of the same size as the networks inputs</exception>
        /// <remarks>
        ///   After the propagation, for each of the 10 network output, if the output was greater than the median response of that output,
        ///   it was assigned +1 (to the hash), otherwise 0. The median was chosen as the threshold to ensure that the network 
        ///   has an even distribution of 0/+1 responses.
        /// </remarks>
        public byte[] ComputeHash(double[] input)
        {
            // Perform check on the nets and input vector length
            if (input.Length != inputsCount)
            {
                throw new NeuralNetworkHashingException("Input vector for hashing should be of the same size as the networks inputs");
            }

            // Propagate through all the networks, compare with median response, assign +1/0.
            int currIndex = 0;
            foreach (Network network in networks)
            {
                double[] output = network.Compute(new BasicNeuralData(input)).Data;
                Array.Copy(PerformOutputNormalization(output, network.MedianResponces), 0, outputVector, currIndex, output.Length);
                currIndex += output.Length;
            }

            return outputVector;
        }

        /// <summary>
        ///   Extracts the hash bins for l hash tables according to the specified Minimal mutual 
        ///   information pattern. The hash bin are extracted from the Output property of the 
        ///   class that correspond to the last outputs of the network ensemble
        /// </summary>
        /// <returns>Array of hashBin locations for l hash tables</returns>
        public long[] ExtractHashBins()
        {
            if (hashPattern == null)
            {
                throw new NeuralNetworkHashingException("There is no pattern to compute the bins on");
            }

            long[] hashBins = new long[hashPattern.NumberOfGroups];
            for (int i = 0; i < hashPattern.NumberOfGroups; i++)
            {
                // For each group, compute the binary representation of the hash bin
                StringBuilder hashBin = new StringBuilder(string.Empty);
                for (int j = 0; j < hashPattern[i].Count; j++)
                {
                    hashBin.Append(OutputVector[hashPattern[i][j]]);
                }

                hashBins[i] = Convert.ToInt32(hashBin.ToString(), 2);
            }

            return hashBins;
        }

        /// <summary>
        ///   Save network ensemble to specified file.
        /// </summary>
        /// <param name = "stream">Stream to save network ensemble into.</param>
        /// <remarks>
        ///   <para>The neural network ensemble is saved using .NET serialization (binary formatter is used).</para>
        /// </remarks>
        public void Save(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
        }

        public void Save(string fileName)
        {
            FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            Save(stream);
            stream.Close();
        }

        private static byte[] PerformOutputNormalization(double[] input, double[] medianResponce)
        {
            byte[] result = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                result[i] = input[i] > medianResponce[i] ? (byte)1 : (byte)0;
            }

            return result;
        }
    }
}