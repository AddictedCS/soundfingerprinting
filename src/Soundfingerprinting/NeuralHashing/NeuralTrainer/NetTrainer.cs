// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using Encog.Engine.Network.Activation;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.Neural.Data.Basic;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Soundfingerprinting.DbStorage;
using Soundfingerprinting.DbStorage.Entities;
using Soundfingerprinting.Fingerprinting;
using Soundfingerprinting.NeuralHashing.Utils;

namespace Soundfingerprinting.NeuralHashing.NeuralTrainer
{
    /// <summary>
    ///   Training callback
    /// </summary>
    /// <param name = "status">Training status</param>
    /// <param name = "correctOutputs">Number of correct outputs</param>
    /// <param name = "currError">Current error [RMS]</param>
    /// <param name = "currIteration">Current iteration</param>
    public delegate void TrainingCallback(TrainingStatus status, double correctOutputs, double currError, int currIteration);

    /// <summary>
    ///   Network trainer. The network is trained according to the algorithm described in 
    ///   'Learning forgiving hash functions: Algorithms and large scale tests', S. Baluja, M. Covell
    /// </summary>
    public class NetTrainer : IDisposable
    {
        #region PrivateMembers

        /// <summary>
        ///   Already disposed parameter
        /// </summary>
        private bool _alreadyDisposed;

        /// <summary>
        ///   Database connection manager
        /// </summary>
        private DaoGateway _dalManager = new DaoGateway(ConfigurationManager.ConnectionStrings["FingerprintConnectionString"].ConnectionString); /*Database manager used to extract the training set*/

        /// <summary>
        ///   Pause semaphore
        /// </summary>
        private Semaphore _pauseSem;

        /// <summary>
        ///   Shows whether training operation is paused
        /// </summary>
        private bool _paused;

        /// <summary>
        ///   Number of training song snippets
        /// </summary>
        private int _trainingSongSnippets = 10; /*Number of training song snippets [10 by default]*/

        /// <summary>
        ///   Working thread
        /// </summary>
        private Thread _workingThread;

        #endregion

        #region Constants

        /// <summary>
        ///   Input layer dimensionality
        /// </summary>
        private const int DEFAULT_FINGERPRINT_SIZE = 4096;

        /// <summary>
        ///   Hidden layer dimensionality
        /// </summary>
        private const int DEFAULT_HIDDEN_NEURONS_COUNT = 41;

        /// <summary>
        ///   Output layer dimensionality
        /// </summary>
        private const int OUT_PUT_NEURONS = 10;

        /// <summary>
        ///   Number of dynamic output reordering cycles
        /// </summary>
        private const int IDYN = 50;

        /// <summary>
        ///   Number of epochs between each output reordering
        /// </summary>
        private const int EDYN = 10;

        /// <summary>
        ///   Number of epochs after reordering cycles
        /// </summary>
        private const int EFIXED = 500;

        #endregion

        #region Propreties

        /// <summary>
        ///   Number of snippets taken from a song for training
        ///   Default value : 10
        /// </summary>
        public int TrainingSongSnippets
        {
            get { return _trainingSongSnippets; }
            set { _trainingSongSnippets = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///   Standard constructor of NetTrainer, should be used in most cases.
        /// </summary>
        /// <param name = "dalManager">Database gateway</param>
        public NetTrainer(DaoGateway dalManager)
        {
            Network net = new Network();
            net.AddLayer(new BasicLayer(new ActivationTANH(), true, DEFAULT_FINGERPRINT_SIZE));
            net.AddLayer(new BasicLayer(new ActivationTANH(), true, DEFAULT_HIDDEN_NEURONS_COUNT));
            net.AddLayer(new BasicLayer(new ActivationTANH(), false, OUT_PUT_NEURONS));
            net.Structure.FinalizeStructure();
            net.Reset();
            Init(net, dalManager);
        }

        /// <summary>
        ///   Common init method for both constructors
        /// </summary>
        /// <param name = "net">Network which will be trained</param>
        /// <param name = "dalManager">Database Fingerprint accessor</param>
        protected void Init(Network net, DaoGateway dalManager)
        {
            _dalManager = dalManager;
            _pauseSem = new Semaphore(0, 1, "PauseSemaphore");
        }

        #endregion

        #region TrainingMethods

        /// <summary>
        ///   Asynchronously starts the process of training the network.
        ///   However keep in mind that only 1 process can be started at a time
        ///   For training multiple networks, create several instances of the class.
        /// </summary>
        /// <returns></returns>
        public void StartTrainingAsync(Network network, TrainingCallback callback)
        {
            if (_alreadyDisposed)
                throw new ObjectDisposedException("Object already disposed");
            Action<Network, TrainingCallback> action = Train;
            action.BeginInvoke(network, callback, action.EndInvoke, action);
        }

        /// <summary>
        ///   Synchronous method for training. Invoked by asynchronous counterpart
        /// </summary>
        public void Train(Network network, TrainingCallback callback)
        {
            IActivationFunction activationFunctionInput = network.GetActivation(0);
            int outputNeurons = network.GetLayerNeuronCount(network.LayerCount - 1);
            double error = 0;
            callback.Invoke(TrainingStatus.FillingStandardInputs, 0, 0, 0); /*First operation is filling standard input/outputs*/
            Dictionary<Int32, List<BasicMLData>> trackIdFingerprints = GetNormalizedTrackFingerprints(activationFunctionInput, _trainingSongSnippets, outputNeurons);
            _workingThread = Thread.CurrentThread;
            IActivationFunction activationFunctionOutput = network.GetActivation(network.LayerCount - 1);
            double[][] normalizedBinaryCodes = GetNormalizedBinaryCodes(activationFunctionOutput, outputNeurons);
            Tuple<double[][], double[][]> tuple = FillStandardInputsOutputs(trackIdFingerprints, normalizedBinaryCodes); /*Fill standard input output*/
            double[][] inputs = tuple.Item1;
            double[][] outputs = tuple.Item2;

            if (inputs == null || outputs == null)
            {
                callback.Invoke(TrainingStatus.Exception, 0, 0, 0);
                return;
            }

            int currentIterration = 0;
            double correctOutputs = 0.0;
            BasicNeuralDataSet dataset = new BasicNeuralDataSet(inputs, outputs);
            ITrain learner = new ResilientPropagation(network, dataset);
            try
            {
                // Dynamic output reordering cycle
                for (int i = 0; i < IDYN; i++) /*Idyn = 50*/
                {
                    if (_paused)
                        _pauseSem.WaitOne();
                    correctOutputs = NetworkPerformanceMeter.MeasurePerformance(network, dataset);
                    callback.Invoke(TrainingStatus.OutputReordering, correctOutputs, error, currentIterration);
                    ReorderOutput(network, dataset, trackIdFingerprints, normalizedBinaryCodes);
                    for (int j = 0; j < EDYN; j++) /*Edyn = 10*/
                    {
                        if (_paused)
                            _pauseSem.WaitOne();
                        correctOutputs = NetworkPerformanceMeter.MeasurePerformance(network, dataset);
                        callback.Invoke(TrainingStatus.RunningDynamicEpoch, correctOutputs, error, currentIterration);
                        learner.Iteration();
                        error = learner.Error;
                        currentIterration++;
                    }
                }

                for (int i = 0; i < EFIXED; i++)
                {
                    if (_paused)
                        _pauseSem.WaitOne();
                    correctOutputs = NetworkPerformanceMeter.MeasurePerformance(network, dataset);
                    callback.Invoke(TrainingStatus.FixedTraining, correctOutputs, error, currentIterration);
                    learner.Iteration();
                    error = learner.Error;
                    currentIterration++;
                }
                network.ComputeMedianResponses(inputs, _trainingSongSnippets);
                callback.Invoke(TrainingStatus.Finished, correctOutputs, error, currentIterration);
            }
            catch (ThreadAbortException)
            {
                callback.Invoke(TrainingStatus.Aborted, correctOutputs, error, currentIterration);
                _paused = false;
            }
        }

        ///<summary>
        ///  Gets tracks/fingerprints from the database
        ///</summary>
        ///<param name = "function">Activation function</param>
        ///<param name = "fingerprintsPerTrack">Fingerprints per track</param>
        ///<param name = "outputs">Number of outputs from the neural network</param>
        ///<returns>Dictionary (Int32 - track id, List - list of corresponding fingerprints</returns>
        public Dictionary<Int32, List<BasicMLData>> GetNormalizedTrackFingerprints(IActivationFunction function, int fingerprintsPerTrack, int outputs)
        {
            List<Track> tracks = _dalManager.ReadTracks();
            Dictionary<Int32, List<Fingerprint>> unnormalized = _dalManager.ReadFingerprintsByMultipleTrackId(tracks, fingerprintsPerTrack);
            Dictionary<Int32, List<BasicMLData>> retVal = new Dictionary<Int32, List<BasicMLData>>();
            int neededTracks = (int) Math.Pow(2, outputs);
            int count = 0;
            foreach (KeyValuePair<Int32, List<Fingerprint>> pair in unnormalized)
            {
                retVal.Add(pair.Key, new List<BasicMLData>());
                foreach (Fingerprint fingerprint in pair.Value)
                    retVal[pair.Key].Add(new BasicMLData(NormalizeUtils.NormalizeDesiredInputInPlace(function, FingerprintManager.DecodeFingerprint(fingerprint.Signature))));
                count++;
                if (count > neededTracks - 1)
                    break;
            }
            return retVal;
        }

        /// <summary>
        ///   Get normalized floating point binary codes
        /// </summary>
        /// <param name = "function">Activation function to normalize</param>
        /// <param name = "binaryLength">Length of the binary length</param>
        /// <returns>Normalized floating point binary codes</returns>
        /// <remarks>
        ///   2^binaryLength normalized binary codes will be returned by the method
        /// </remarks>
        public double[][] GetNormalizedBinaryCodes(IActivationFunction function, int binaryLength)
        {
            byte[][] codes = BinaryOutputUtil.GetAllBinaryCodes(binaryLength);
            int length = codes.GetLength(0);
            double[][] fCodes = new double[length][];
            for (int i = 0; i < length; i++)
            {
                fCodes[i] = Array.ConvertAll(codes[i], (s) => (double) s);
                NormalizeUtils.NormalizeOneDesiredOutputInPlace(function, fCodes[i]);
            }
            return fCodes;
        }

        /// <summary>
        ///   Fills inputs, outputs with the corresponding data that will be trained.
        /// </summary>
        /// <param name = "trackIdFingerprints">Track id fingerprints data structure</param>
        /// <param name = "binaryCodes">Normalized binary codes</param>
        /// <returns>A tuple representing inputs/outputs</returns>
        /// <exception cref = "NetTrainerException">Throws when there is not enough snippets for a given song</exception>
        /// <remarks>
        ///   All fingerprints for a specific track will point to the same binary signature
        /// </remarks>
        public Tuple<double[][], double[][]> FillStandardInputsOutputs(Dictionary<Int32, List<BasicMLData>> trackIdFingerprints, double[][] binaryCodes)
        {
            int trackCount = trackIdFingerprints.Count;
            //Check availability of binary outputs with respect to tracks
            if (binaryCodes.GetLength(0) > trackCount)
                throw new NetTrainerException("Not enough songs in the database for training purpose");

            int inOutIndex = 0;
            double[][] inputs = new double[trackCount*_trainingSongSnippets][];
            double[][] outputs = new double[trackCount*_trainingSongSnippets][];
            int count = 0;

            //Assign all fingerprints of a single song with one binary code.
            foreach (KeyValuePair<Int32, List<BasicMLData>> pair in trackIdFingerprints)
            {
                List<BasicMLData> fingerprints = pair.Value;
                if (fingerprints.Count < _trainingSongSnippets)
                    throw new NetTrainerException("Not enough fingerprints for a specific song. Song Int32:" + pair.Key);

                foreach (BasicMLData fingerprint in fingerprints)
                {
                    inputs[inOutIndex] = fingerprint.Data;
                    if (inputs[inOutIndex] == null) throw new NetTrainerException("Inputs to be trained cannon be null");
                    outputs[inOutIndex] = binaryCodes[count]; /*All snippets from the same song must have the same output*/
                    inOutIndex++;
                }
                count++;
            }
            return new Tuple<double[][], double[][]>(inputs, outputs);
        }


        /// <summary>
        ///   Reorders the outputs of the network training process according to network response on the current stage of learning
        /// </summary>
        /// <param name = "network">Network that is trained</param>
        /// <param name = "dataset"> Dataset with the data [input/ideal]</param>
        /// <param name = "trackIdFingerprints">Tracks and their associated fingerprints</param>
        /// <param name = "binaryCodes">Normalized binary codes</param>
        protected void ReorderOutput(Network network, BasicNeuralDataSet dataset, Dictionary<Int32, List<BasicMLData>> trackIdFingerprints, double[][] binaryCodes)
        {
            int outputNeurons = network.GetLayerNeuronCount(network.LayerCount - 1);
            int trackCount = trackIdFingerprints.Count;
            //For each song, compute Am
            double[][] am = new double[trackCount][];
            int counter = 0;
            foreach (KeyValuePair<Int32, List<BasicMLData>> pair in trackIdFingerprints)
            {
                List<BasicMLData> sxSnippet = pair.Value;
                if (sxSnippet.Count < _trainingSongSnippets) throw new NetTrainerException("Not enough snippets for a song");
                am[counter] = new double[outputNeurons];
                foreach (BasicMLData snippet in sxSnippet)
                {
                    IMLData actualOutput = network.Compute(snippet);
                    for (int k = 0; k < outputNeurons; k++)
                    {
                        actualOutput[k] /= outputNeurons;
                        am[counter][k] += actualOutput[k];
                    }
                }
                counter++;
            }

            //Get a collection of tracks (shallow copy)
            Int32[] unassignedTracks = new Int32[trackCount];
            int countTrack = 0;
            foreach (KeyValuePair<Int32, List<BasicMLData>> item in trackIdFingerprints)
                unassignedTracks[countTrack++] = item.Key;

            int currItteration = 0;
            // Find binary code - track pair that has min l2 norm across all binary codes
            List<Tuple<int, int>> binCodeTrackPair = BinaryOutputUtil.FindMinL2Norm(binaryCodes, am);
            foreach (Tuple<int, int> pair in binCodeTrackPair)
            {
                // Set the input-output for all fingerprints of that song
                List<BasicMLData> songFingerprints = trackIdFingerprints[unassignedTracks[pair.Item2]];
                foreach (BasicMLData songFingerprint in songFingerprints)
                {
                    for (int i = 0, n = songFingerprint.Count; i < n; i++)
                        dataset.Data[currItteration].Input[i] = songFingerprint[i];
                    for (int i = 0, n = binaryCodes[pair.Item1].Length; i < n; i++)
                        dataset.Data[currItteration].Ideal[i] = binaryCodes[pair.Item1][i];
                    currItteration++;
                }
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        ///   Disposes resources associated with this object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        ///   Finalizer
        /// </summary>
        ~NetTrainer()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_alreadyDisposed)
            {
                if (disposing)
                {
                    //dispose managed resources
                    _pauseSem.Close();
                }
                //dispose unmanaged resources
                _alreadyDisposed = true;
            }
        }

        /// <summary>
        ///   Pauses current training process
        /// </summary>
        public void PauseTraining()
        {
            if (_alreadyDisposed)
                throw new ObjectDisposedException("Object already disposed");
            if (!_paused)
            {
                _paused = true;
            }
        }

        /// <summary>
        ///   Resumes paused training
        /// </summary>
        public void ResumeTraining()
        {
            if (_alreadyDisposed)
                throw new ObjectDisposedException("Object already disposed");
            if (_paused)
            {
                _paused = false;
                _pauseSem.Release();
            }
        }

        /// <summary>
        ///   Aborts the current process of training
        /// </summary>
        public void AbortTraining()
        {
            if (_alreadyDisposed)
                throw new ObjectDisposedException("Object already disposed");
            _workingThread.Abort();
        }
    }
}