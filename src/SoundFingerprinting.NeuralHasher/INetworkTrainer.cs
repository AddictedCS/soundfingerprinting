namespace SoundFingerprinting.NeuralHasher
{
    using System.Threading.Tasks;

    public interface INetworkTrainer
    {
        Task<Network> Train(NetworkConfiguration networkConfiguration, int numberOfTracks, int[] spectralImagesIndexesToConsider, TrainingCallback callback);
    }
}