namespace SoundFingerprinting.NeuralHasher.NeuralTrainer
{
    using Encog.Engine.Network.Activation;
    using Encog.Neural.Data.Basic;
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

        private readonly IDynamicReorderingAlgorithm dynamicReorderingAlgorithm;

        public NetTrainer(IModelService modelService)
            : this(new TrainingDataProvider(modelService, DependencyResolver.Current.Get<IBinaryOutputHelper>()), DependencyResolver.Current.Get<INetworkFactory>(), DependencyResolver.Current.Get<INormalizeStrategy>(), DependencyResolver.Current.Get<IDynamicReorderingAlgorithm>())
        {
            TrainingSongSnippets = 10;
        }

        internal NetTrainer(ITrainingDataProvider trainingDataProvider, INetworkFactory networkFactory, INormalizeStrategy normalizeStrategy, IDynamicReorderingAlgorithm dynamicReorderingAlgorithm)
        {
            this.trainingDataProvider = trainingDataProvider;
            this.networkFactory = networkFactory;
            this.normalizeStrategy = normalizeStrategy;
            this.dynamicReorderingAlgorithm = dynamicReorderingAlgorithm;
        }

        public int TrainingSongSnippets { get; set; }

        public Network Train(int numberOfTracks, int[] spectralImagesIndexesToConsider, IActivationFunction activationFunction, TrainingCallback callback)
        {
            var network = networkFactory.Create(activationFunction, DefaultFingerprintSize, DefaultHiddenNeuronsCount, OutputNeurons);
            var spectralImagesToTrain = trainingDataProvider.GetSpectralImagesToTrain(
                spectralImagesIndexesToConsider, numberOfTracks);
            var trainingSet = trainingDataProvider.MapSpectralImagesToBinaryOutputs(
                spectralImagesToTrain, numberOfTracks);
            normalizeStrategy.NormalizeInputInPlace(activationFunction, trainingSet.Inputs);
            normalizeStrategy.NormalizeOutputInPlace(activationFunction, trainingSet.Outputs);
            var dataset = new BasicNeuralDataSet(trainingSet.Inputs, trainingSet.Outputs);
            var learner = new ResilientPropagation(network, dataset);
            int iteration = 0;
            double correctOutputs = 0.0;
            double error = 0;        
            for (int i = 0; i < Idyn; i++)
            {
                correctOutputs = NetworkPerformanceMeter.MeasurePerformance(network, dataset);
                callback.Invoke(TrainingStatus.OutputReordering, correctOutputs, error, iteration);
                var am = dynamicReorderingAlgorithm.ComputeAm(network, spectralImagesToTrain, numberOfTracks);
                var normPairs = dynamicReorderingAlgorithm.CalculateL2NormPairs(trainingSet.Outputs, am);
                var bestPairs = dynamicReorderingAlgorithm.FindBestReorderingPairs(normPairs);
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

                // Edyn = 10
                for (int j = 0; j < Edyn; j++)
                {
                    correctOutputs = NetworkPerformanceMeter.MeasurePerformance(network, dataset);
                    callback.Invoke(TrainingStatus.RunningDynamicEpoch, correctOutputs, error, iteration);
                    learner.Iteration();
                    error = learner.Error;
                    iteration++;
                }
            }

            for (int i = 0; i < Efixed; i++)
            {
                correctOutputs = NetworkPerformanceMeter.MeasurePerformance(network, dataset);
                callback.Invoke(TrainingStatus.FixedTraining, correctOutputs, error, iteration);
                learner.Iteration();
                error = learner.Error;
                iteration++;
            }

            network.ComputeMedianResponses(trainingSet.Inputs, TrainingSongSnippets);
            callback.Invoke(TrainingStatus.Finished, correctOutputs, error, iteration);
            return network;
        }
    }
}