namespace SoundFingerprinting.NeuralHasher
{
    using System.Collections.Generic;

    public interface ITrainingDataProvider
    {
        List<double[][]> GetSpectralImagesToTrain(int[] spectralImageIndexsToConsider, int numberOfTracks);

        TrainingSet FillStandardInputsOutputs(List<double[][]> spectralImagesToTrain, int binaryOutputsCount);
    }
}