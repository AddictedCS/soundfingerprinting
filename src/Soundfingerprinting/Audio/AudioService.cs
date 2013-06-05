namespace Soundfingerprinting.Audio
{
    public abstract class AudioService : IAudioService
    {
        public abstract void Dispose();

        public abstract float[] ReadMonoFromFile(string pathToFile, int sampleRate, int millisecondsToRead, int startAtMillisecond);

        public float[] ReadMonoFromFile(string pathToFile, int sampleRate)
        {
            return ReadMonoFromFile(pathToFile, sampleRate, 0, 0);
        }
    }
}