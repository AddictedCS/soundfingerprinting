namespace SoundFingerprinting.NeuralHasher
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO;

    public interface ITrainingDataProvider
    {
        Dictionary<IModelReference, double[][]> GetSpectralImagesToTrain(int[] spectralImageIndexsToConsider, int numberOfTracks);
    }
}