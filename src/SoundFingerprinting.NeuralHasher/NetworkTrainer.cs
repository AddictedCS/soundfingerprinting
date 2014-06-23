namespace SoundFingerprinting.NeuralHasher
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Encog.Neural.Data.Basic;
    using Encog.Neural.Networks.Training.Propagation.Back;

    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.NeuralHasher.NeuralTrainer;
    using SoundFingerprinting.NeuralHasher.Utils;

    public delegate void TrainingCallback(TrainingStatus status, double correctOutputs, double errorRate, int iteration);

    public class NetworkTrainer : INetworkTrainer 
    {
        private const int DefaultFingerprintSize = 128 * 32;

        private const int Idyn = 50;

        private const int Edyn = 10;

        private const int Efixed = 500;

        private readonly ITrainingDataProvider trainingDataProvider;

        private readonly INetworkFactory networkFactory;

        private readonly INormalizeStrategy normalizeStrategy;

        private readonly IDynamicReorderingAlgorithm dynamicReorderingAlgorithm;

        private readonly NetworkPerformanceMeter networkPerformanceMeter;

        public NetworkTrainer(IModelService modelService)
            : this(new TrainingDataProvider(modelService, DependencyResolver.Current.Get<IBinaryOutputHelper>()), DependencyResolver.Current.Get<INetworkFactory>(), DependencyResolver.Current.Get<INormalizeStrategy>(), DependencyResolver.Current.Get<IDynamicReorderingAlgorithm>())
        {
            TrainingSongSnippets = 10;
        }

        internal NetworkTrainer(ITrainingDataProvider trainingDataProvider, INetworkFactory networkFactory, INormalizeStrategy normalizeStrategy, IDynamicReorderingAlgorithm dynamicReorderingAlgorithm)
        {
            this.trainingDataProvider = trainingDataProvider;
            this.networkFactory = networkFactory;
            this.normalizeStrategy = normalizeStrategy;
            this.dynamicReorderingAlgorithm = dynamicReorderingAlgorithm;
            networkPerformanceMeter = new NetworkPerformanceMeter();
        }

        public int TrainingSongSnippets { get; set; }

        public Task<Network> Train(NetworkConfiguration networkConfiguration, int numberOfTracks, int[] spectralImagesIndexesToConsider, TrainingCallback callback)
        {
            var network = networkFactory.Create(
                networkConfiguration.ActivationFunction, 
                DefaultFingerprintSize, 
                networkConfiguration.HiddenLayerCount, 
                networkConfiguration.OutputCount);

            var spectralImagesToTrain = trainingDataProvider.GetSpectralImagesToTrain(
                spectralImagesIndexesToConsider, numberOfTracks);
            var trainingSet = trainingDataProvider.MapSpectralImagesToBinaryOutputs(
                spectralImagesToTrain, networkConfiguration.OutputCount);

            normalizeStrategy.NormalizeInputInPlace(networkConfiguration.ActivationFunction, trainingSet.Inputs);
            normalizeStrategy.NormalizeOutputInPlace(networkConfiguration.ActivationFunction, trainingSet.Outputs);
            return Task.Factory.StartNew(
                () =>
                    {
                        var dataset = new BasicNeuralDataSet(trainingSet.Inputs, trainingSet.Outputs);
                        var learner = new Backpropagation(network, dataset);
                        double correctOutputs = 0.0;
                        for (int idynIndex = 0; idynIndex < Idyn; idynIndex++)
                        {
                            correctOutputs = networkPerformanceMeter.MeasurePerformance(
                                network, dataset, networkConfiguration.ActivationFunction);
                            callback(TrainingStatus.OutputReordering, correctOutputs, learner.Error, idynIndex * Edyn);
                            var bestPairs = GetBestPairsForReordering(
                                numberOfTracks, network, spectralImagesToTrain, trainingSet);
                            ReorderOutputsAccordingToBestPairs(bestPairs, trainingSet, dataset);

                            // Edyn = 10
                            for (int edynIndex = 0; edynIndex < Edyn; edynIndex++)
                            {
                                correctOutputs = networkPerformanceMeter.MeasurePerformance(
                                    network, dataset, networkConfiguration.ActivationFunction);
                                callback(
                                    TrainingStatus.RunningDynamicEpoch, 
                                    correctOutputs, 
                                    learner.Error, 
                                    (idynIndex * Edyn) + edynIndex);
                                learner.Iteration();
                            }
                        }

                        for (int efixedIndex = 0; efixedIndex < Efixed; efixedIndex++)
                        {
                            correctOutputs = networkPerformanceMeter.MeasurePerformance(
                                network, dataset, networkConfiguration.ActivationFunction);
                            callback(
                                TrainingStatus.FixedTraining, correctOutputs, learner.Error, (Idyn * Edyn) + efixedIndex);
                            learner.Iteration();
                        }

                        network.ComputeMedianResponses(trainingSet.Inputs, TrainingSongSnippets);
                        callback(TrainingStatus.Finished, correctOutputs, learner.Error, (Idyn * Edyn) + Efixed);
                        return network;
                    });
        }

        private IEnumerable<BestReorderingPair> GetBestPairsForReordering(
            int numberOfTracks, Network network, List<double[][]> spectralImagesToTrain, TrainingSet trainingSet)
        {
            var am = dynamicReorderingAlgorithm.ComputeAm(network, spectralImagesToTrain, numberOfTracks);
            var normPairs = dynamicReorderingAlgorithm.CalculateL2NormPairs(trainingSet.Outputs, am);
            var bestPairs = dynamicReorderingAlgorithm.FindBestReorderingPairs(normPairs);
            return bestPairs;
        }

        private void ReorderOutputsAccordingToBestPairs(IEnumerable<BestReorderingPair> bestPairs, TrainingSet trainingSet, BasicNeuralDataSet dataset)
        {
            int inputIndex = 0;
            foreach (var bestPair in bestPairs)
            {
                for (int j = 0, n = trainingSet.Inputs[bestPair.SnippetIndex].Length; j < n; j++)
                {
                    dataset.Data[inputIndex].Input[j] = trainingSet.Inputs[bestPair.SnippetIndex][j];
                }

                for (int j = 0, n = trainingSet.Outputs[bestPair.BinaryOutputIndex].Length; j < n; j++)
                {
                    dataset.Data[inputIndex].Ideal[j] = trainingSet.Outputs[bestPair.BinaryOutputIndex][j];
                }

                inputIndex++;
            }
        }
    }
}