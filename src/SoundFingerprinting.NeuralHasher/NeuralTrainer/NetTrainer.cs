namespace SoundFingerprinting.NeuralHasher.NeuralTrainer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using Encog.Engine.Network.Activation;
    using Encog.ML.Data;
    using Encog.ML.Data.Basic;
    using Encog.Neural.Data.Basic;
    using Encog.Neural.Networks.Layers;
    using Encog.Neural.Networks.Training;
    using Encog.Neural.Networks.Training.Propagation.Resilient;

    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.NeuralHasher.Utils;

    public delegate void TrainingCallback(TrainingStatus status, double correctOutputs, double errorRate, int iteration);

    public class NetTrainer
    {
        private const int DefaultFingerprintSize = 128 * 32;

        private const int DefaultHiddenNeuronsCount = 41;

        private const int OutputNeurons = 10;

        private const int Idyn = 50;

        private const int Edyn = 10;

        private const int Efixed = 500;

        private readonly ITrainingDataProvider trainingDataProvider;

        private readonly INetworkFactory networkFactory;

        private readonly INormalizeStrategy normalizeStrategy;

        public NetTrainer(IModelService modelService)
            : this(new TrainingDataProvider(modelService, DependencyResolver.Current.Get<IBinaryOutputHelper>()), DependencyResolver.Current.Get<INetworkFactory>(), DependencyResolver.Current.Get<INormalizeStrategy>())
        {
            TrainingSongSnippets = 10;
        }

        internal NetTrainer(ITrainingDataProvider trainingDataProvider, INetworkFactory networkFactory, INormalizeStrategy normalizeStrategy)
        {
            this.trainingDataProvider = trainingDataProvider;
            this.networkFactory = networkFactory;
            this.normalizeStrategy = normalizeStrategy;
        }

        public int TrainingSongSnippets { get; set; }

        public Network Train(int numberOfTracks, int[] spectralImagesIndexesToConsider, IActivationFunction activationFunction, TrainingCallback callback)
        {
            var network = networkFactory.Create(activationFunction, DefaultFingerprintSize, DefaultHiddenNeuronsCount, OutputNeurons);
            var trainingSet = trainingDataProvider.GetTrainingSet(spectralImagesIndexesToConsider, numberOfTracks);
            normalizeStrategy.NormalizeInputInPlace(activationFunction, trainingSet.Inputs);
            normalizeStrategy.NormalizeOutputInPlace(activationFunction, trainingSet.Outputs);
            var dataset = new BasicNeuralDataSet(trainingSet.Inputs, trainingSet.Outputs);
            var learner = new ResilientPropagation(network, dataset);
            int currentIterration = 0;
            double correctOutputs = 0.0;
         
            // Dynamic output reordering cycle
            // Idyn = 50
            /*
            for (int i = 0; i < Idyn; i++)
            {
                correctOutputs = NetworkPerformanceMeter.MeasurePerformance(network, dataset);
                callback.Invoke(TrainingStatus.OutputReordering, correctOutputs, error, currentIterration);
                ReorderOutput(network, dataset, trackIdFingerprints, normalizedBinaryCodes);
                // Edyn = 10
                for (int j = 0; j < Edyn; j++)
                {
                    correctOutputs = NetworkPerformanceMeter.MeasurePerformance(network, dataset);
                    callback.Invoke(TrainingStatus.RunningDynamicEpoch, correctOutputs, error, currentIterration);
                    learner.Iteration();
                    error = learner.Error;
                    currentIterration++;
                }
            }

            for (int i = 0; i < Efixed; i++)
            {
                correctOutputs = NetworkPerformanceMeter.MeasurePerformance(network, dataset);
                callback.Invoke(TrainingStatus.FixedTraining, correctOutputs, error, currentIterration);
                learner.Iteration();
                error = learner.Error;
                currentIterration++;
            }

            network.ComputeMedianResponses(inputs, TrainingSongSnippets);
            callback.Invoke(TrainingStatus.Finished, correctOutputs, error, currentIterration);
            
             */ return network;
        }

        protected void ReorderOutput(Network network, BasicNeuralDataSet dataset, Dictionary<int, List<BasicMLData>> trackIdFingerprints, double[][] binaryCodes)
        {
            int outputNeurons = network.GetLayerNeuronCount(network.LayerCount - 1);
            int trackCount = trackIdFingerprints.Count;

            // For each song, compute Am
            double[][] am = new double[trackCount][];
            int counter = 0;
            foreach (KeyValuePair<int, List<BasicMLData>> pair in trackIdFingerprints)
            {
                List<BasicMLData> sxSnippet = pair.Value;
                if (sxSnippet.Count < TrainingSongSnippets)
                {
                    throw new Exception("Not enough snippets for a song");
                }

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

            // Get a collection of tracks (shallow copy)
            int[] unassignedTracks = new int[trackCount];
            int countTrack = 0;
            foreach (KeyValuePair<int, List<BasicMLData>> item in trackIdFingerprints)
            {
                unassignedTracks[countTrack++] = item.Key;
            }

            int currItteration = 0;

            // Find binary code - track pair that has min l2 norm across all binary codes
            List<Tuple<int, int>> binCodeTrackPair = new List<Tuple<int, int>>(); // = bin .FindMinL2Norm(binaryCodes, am);
            foreach (Tuple<int, int> pair in binCodeTrackPair)
            {
                // Set the input-output for all fingerprints of that song
                List<BasicMLData> songFingerprints = trackIdFingerprints[unassignedTracks[pair.Item2]];
                foreach (BasicMLData songFingerprint in songFingerprints)
                {
                    for (int i = 0, n = songFingerprint.Count; i < n; i++)
                    {
                        dataset.Data[currItteration].Input[i] = songFingerprint[i];
                    }

                    for (int i = 0, n = binaryCodes[pair.Item1].Length; i < n; i++)
                    {
                        dataset.Data[currItteration].Ideal[i] = binaryCodes[pair.Item1][i];
                    }

                    currItteration++;
                }
            }
        }
    }
}