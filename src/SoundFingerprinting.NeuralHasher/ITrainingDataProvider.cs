namespace SoundFingerprinting.NeuralHasher
{
    using System.Collections.Generic;

    public interface ITrainingDataProvider
    {
        List<double[][]> GetSpectralImagesToTrain(int[] spectralImageIndexsToConsider, int numberOfTracks);

        TrainingSet MapSpectralImagesToBinaryOutputs(List<double[][]> spectralImagesToTrain, int binaryOutputsCount);

        TrainingSet GetTrainingSet(int[] spectralImageIndexesToConsider, int numberOfTracks);
    }
}