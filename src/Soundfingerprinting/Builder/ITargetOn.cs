namespace Soundfingerprinting.Builder
{
    public interface ITargetOn
    {
        IWithConfiguration On(string pathToAudioFile);

        IWithConfiguration On(float[] audioSamples);

        IWithConfiguration On(string pathToAudioFile, int secondsToProcess, int startAtSecond);
    }
}