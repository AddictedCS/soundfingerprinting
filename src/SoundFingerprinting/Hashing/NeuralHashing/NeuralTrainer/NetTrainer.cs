namespace SoundFingerprinting.Hashing.NeuralHashing.NeuralTrainer
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using Encog.Engine.Network.Activation;
    using Encog.ML.Data;
    using Encog.ML.Data.Basic;
    using Encog.Neural.Data.Basic;
    using Encog.Neural.Networks.Layers;
    using Encog.Neural.Networks.Training;
    using Encog.Neural.Networks.Training.Propagation.Resilient;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.Entities;
    using SoundFingerprinting.Hashing.NeuralHashing.Utils;
    using SoundFingerprinting.Utils;

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
        /// <summary>
        ///   Input layer dimensionality
        /// </summary>
        private const int DefaultFingerprintSize = 4096;

        /// <summary>
        ///   Hidden layer dimensionality
        /// </summary>
        private const int DefaultHiddenNeuronsCount = 41;

        /// <summary>
        ///   Output layer dimensionality
        /// </summary>
        private const int OutPutNeurons = 10;

        /// <summary>
        ///   Number of dynamic output reordering cycles
        /// </summary>
        private const int Idyn = 50;

        /// <summary>
        ///   Number of epochs between each output reordering
        /// </summary>
        private const int Edyn = 10;

        /// <summary>
        ///   Number of epochs after reordering cycles
        /// </summary>
        private const int Efixed = 500;

        private readonly IModelService modelService;

        private readonly Semaphore pauseSem;

        private bool alreadyDisposed;

        private bool paused;

        private int trainingSongSnippets = 10; /*Number of training song snippets [10 by default]*/
    
        private Thread workingThread;

        public NetTrainer(IModelService modelService)
        {
            Network net = new Network();
            net.AddLayer(new BasicLayer(new ActivationTANH(), true, DefaultFingerprintSize));
            net.AddLayer(new BasicLayer(new ActivationTANH(), true, DefaultHiddenNeuronsCount));
            net.AddLayer(new BasicLayer(new ActivationTANH(), false, OutPutNeurons));
            net.Structure.FinalizeStructure();
            net.Reset();
            this.modelService = modelService;
            pauseSem = new Semaphore(0, 1, "PauseSemaphore");
        }

        ~NetTrainer()
        {
            Dispose(false);
        }

        public int TrainingSongSnippets
        {
            get { return trainingSongSnippets; }
            set { trainingSongSnippets = value; }
        }

        /// <summary>
        ///   Asynchronously starts the process of training the network.
        ///   However keep in mind that only 1 process can be started at a time
        ///   For training multiple networks, create several instances of the class.
        /// </summary>
        /// <returns></returns>
        public void StartTrainingAsync(Network network, TrainingCallback callback)
        {
            if (alreadyDisposed)
            {
                throw new ObjectDisposedException("Object already disposed");
            }

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
            Dictionary<Int32, List<BasicMLData>> trackIdFingerprints = GetNormalizedTrackFingerprints(activationFunctionInput, trainingSongSnippets, outputNeurons);
            workingThread = Thread.CurrentThread;
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
                for (int i = 0; i < Idyn; i++) /*Idyn = 50*/
                {
                    if (paused)
                        pauseSem.WaitOne();
                    correctOutputs = NetworkPerformanceMeter.MeasurePerformance(network, dataset);
                    callback.Invoke(TrainingStatus.OutputReordering, correctOutputs, error, currentIterration);
                    ReorderOutput(network, dataset, trackIdFingerprints, normalizedBinaryCodes);
                    for (int j = 0; j < Edyn; j++) /*Edyn = 10*/
                    {
                        if (paused)
                            pauseSem.WaitOne();
                        correctOutputs = NetworkPerformanceMeter.MeasurePerformance(network, dataset);
                        callback.Invoke(TrainingStatus.RunningDynamicEpoch, correctOutputs, error, currentIterration);
                        learner.Iteration();
                        error = learner.Error;
                        currentIterration++;
                    }
                }

                for (int i = 0; i < Efixed; i++)
                {
                    if (paused)
                        pauseSem.WaitOne();
                    correctOutputs = NetworkPerformanceMeter.MeasurePerformance(network, dataset);
                    callback.Invoke(TrainingStatus.FixedTraining, correctOutputs, error, currentIterration);
                    learner.Iteration();
                    error = learner.Error;
                    currentIterration++;
                }
                network.ComputeMedianResponses(inputs, trainingSongSnippets);
                callback.Invoke(TrainingStatus.Finished, correctOutputs, error, currentIterration);
            }
            catch (ThreadAbortException)
            {
                callback.Invoke(TrainingStatus.Aborted, correctOutputs, error, currentIterration);
                paused = false;
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
            IList<Track> tracks = modelService.ReadTracks();
            IDictionary<int, IList<Fingerprint>> unnormalized = modelService.ReadFingerprintsByMultipleTrackId(tracks, fingerprintsPerTrack);
            Dictionary<Int32, List<BasicMLData>> retVal = new Dictionary<Int32, List<BasicMLData>>();
            int neededTracks = (int) Math.Pow(2, outputs);
            int count = 0;

            FingerprintDescriptor descriptor = new FingerprintDescriptor();
            foreach (var pair in unnormalized)
            {
                retVal.Add(pair.Key, new List<BasicMLData>());
                foreach (Fingerprint fingerprint in pair.Value)
                    retVal[pair.Key].Add(new BasicMLData(NormalizeUtils.NormalizeDesiredInputInPlace(function, descriptor.DecodeFingerprint(fingerprint.Signature))));
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
            double[][] inputs = new double[trackCount*trainingSongSnippets][];
            double[][] outputs = new double[trackCount*trainingSongSnippets][];
            int count = 0;

            //Assign all fingerprints of a single song with one binary code.
            foreach (KeyValuePair<Int32, List<BasicMLData>> pair in trackIdFingerprints)
            {
                List<BasicMLData> fingerprints = pair.Value;
                if (fingerprints.Count < trainingSongSnippets)
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        public void PauseTraining()
        {
            if (alreadyDisposed)
                throw new ObjectDisposedException("Object already disposed");
            if (!paused)
            {
                paused = true;
            }
        }

        public void ResumeTraining()
        {
            if (alreadyDisposed)
            {
                throw new ObjectDisposedException("Object already disposed");
            }

            if (paused)
            {
                paused = false;
                pauseSem.Release();
            }
        }

        public void AbortTraining()
        {
            if (alreadyDisposed)
            {
                throw new ObjectDisposedException("Object already disposed");
            }

            workingThread.Abort();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!alreadyDisposed)
            {
                if (disposing)
                {
                    //dispose managed resources
                    pauseSem.Close();
                }
                //dispose unmanaged resources
                alreadyDisposed = true;
            }
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
                if (sxSnippet.Count < trainingSongSnippets) throw new NetTrainerException("Not enough snippets for a song");
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
    }
}