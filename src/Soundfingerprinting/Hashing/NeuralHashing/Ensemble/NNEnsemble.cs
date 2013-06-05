namespace Soundfingerprinting.Hashing.NeuralHashing.Ensemble
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;

    using Encog.Neural.Data.Basic;

    using Soundfingerprinting.Hashing.NeuralHashing.MMI;

    /// <summary>
    ///   Neural network ensemble used in hashing the fingerprints
    /// </summary>
    [Serializable]
// ReSharper disable InconsistentNaming
    public class NNEnsemble
// ReSharper restore InconsistentNaming
    {
        #region PrivateMembers

        /// <summary>
        ///   Inputs count
        /// </summary>
        private readonly int _inputsCount;

        /// <summary>
        ///   All networks used in computation
        /// </summary>
        private readonly Network[] _networks;

        /// <summary>
        ///   Output vector
        /// </summary>
        private readonly byte[] _outputVector;

        /// <summary>
        ///   Total outputs
        /// </summary>
        private readonly int _totalOutputs;

        /// <summary>
        ///   Hash pattern
        /// </summary>
        private MinimalMutualInfoPattern _hashPattern;

        #endregion

        #region Propreties

        /// <summary>
        ///   Output vector
        /// </summary>
        public byte[] OutputVector
        {
            get { return _outputVector; }
        }

        /// <summary>
        ///   Hash pattern
        /// </summary>
        public MinimalMutualInfoPattern HashPattern
        {
            set
            {
                if (value.NNEnsembleDimensionality != _networks[0].OutputCount * _networks.Length)
                {
                    throw new ArgumentException("The hash patter does not correspond to networks of the NNEnsemble");
                }

                _hashPattern = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name = "networks">An array of trained networks that will work as hash functions</param>
        /// <exception cref = "NNHashingException">Can not create a NN Ensemble out of no networks</exception>
        public NNEnsemble(Network[] networks)
        {
            if (networks == null || networks.Length == 0)
                throw new NNHashingException("Can not create a NN Ensemble out of no networks");
            _inputsCount = networks[0].InputCount;
            //Check all inputs
            foreach (Network network in networks)
            {
                if (network.InputCount != _inputsCount)
                    throw new NNHashingException("All networks should have the same inputs count");
                if (network.MedianResponces == null)
                    throw new NNHashingException("Median responses of one of the networks is null," +
                                                 " please run Median Responses computation for each of the networks and try again");
            }
            _networks = networks;

            foreach (Network network in _networks)
            {
                _totalOutputs += network.GetLayerNeuronCount(network.LayerCount - 1);
            }
            _outputVector = new byte[_totalOutputs];
        }

        #endregion

        /// <summary>
        ///   Propagates the input vector through all of the networks of the ensemble
        /// </summary>
        /// <param name = "input">Input vector</param>
        /// <returns>Hashed vector</returns>
        /// <exception cref = "NNHashingException">Input vector should be of the same size as the networks inputs</exception>
        /// <remarks>
        ///   After the propagation, for each of the 10 network output, if the output was greater than the median response of that output,
        ///   it was assigned +1 (to the hash), otherwise 0. The median was chosen as the threshold to ensure that the network 
        ///   has an even distribution of 0/+1 responses.
        /// </remarks>
        public byte[] ComputeHash(double[] input)
        {
            //Perform check on the nets and input vector length
            if (input.Length != _inputsCount) throw new NNHashingException("Input vector for hashing should be of the same size as the networks inputs");

            //Propagate through all the networks, compare with median response, assign +1/0.
            int currIndex = 0;
            foreach (Network network in _networks)
            {
                double[] output = network.Compute(new BasicNeuralData(input)).Data;
                Array.Copy(PerformOutputNormalization(output, network.MedianResponces), 0, _outputVector, currIndex, output.Length);
                currIndex += output.Length;
            }
            return _outputVector;
        }

        /// <summary>
        ///   Extracts the hash bins for l hash tables according to the specified Minimal mutual 
        ///   information pattern. The hash bin are extracted from the Output property of the 
        ///   class that correspond to the last outputs of the network ensemble
        /// </summary>
        /// <returns>Array of hashBin locations for l hash tables</returns>
        public long[] ExtractHashBins()
        {
            if (_hashPattern == null) throw new NNHashingException("There is no pattern to compute the bins on");
            long[] hashBins = new long[_hashPattern.NumberOfGroups];
            for (int i = 0; i < _hashPattern.NumberOfGroups; i++)
            {
                //For each group, compute the binary representation of the hash bin
                StringBuilder hashBin = new StringBuilder("");
                for (int j = 0; j < _hashPattern[i].Count; j++)
                {
                    hashBin.Append(OutputVector[_hashPattern[i][j]]);
                }
                hashBins[i] = Convert.ToInt32(hashBin.ToString(), 2);
            }
            return hashBins;
        }

        private static byte[] PerformOutputNormalization(double[] input, double[] medianResponce)
        {
            byte[] result = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                result[i] = input[i] > medianResponce[i] ? (byte) 1 : (byte) 0;
            }
            return result;
        }

        #region I/O Methods

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

        /// <summary>
        ///   Load network ensemble from specified file.
        /// </summary>
        /// <param name = "fileName">File name to load network ensemble from.</param>
        /// <returns>Returns instance of <see cref = "NNEnsemble" /> class with all properties initialized from file.</returns>
        /// <remarks>
        ///   <para>Neural network ensemble is loaded from file using .NET serialization (binary formater is used).</para>
        /// </remarks>
        public static NNEnsemble Load(string fileName)
        {
            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            NNEnsemble network = Load(stream);
            stream.Close();

            return network;
        }

        /// <summary>
        ///   Load network from specified file.
        /// </summary>
        /// <param name = "stream">Stream to load network from.</param>
        /// <returns>Returns instance of <see cref = "NNEnsemble" /> class with all properties initialized from file.</returns>
        /// <remarks>
        ///   <para>Neural network is loaded from file using .NET serialization (binary formatter is used).</para>
        /// </remarks>
        public static NNEnsemble Load(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            NNEnsemble network = (NNEnsemble) formatter.Deserialize(stream);
            return network;
        }

        #endregion
    }
}