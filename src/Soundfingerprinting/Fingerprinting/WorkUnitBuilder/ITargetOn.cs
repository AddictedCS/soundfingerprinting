namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder
{
    public interface ITargetOn
    {
        IWithConfiguration On(string pathToAudioFile);

        IWithConfiguration On(float[] audioSamples);

        IWithConfiguration On(string pathToAudioFile, int millisecondsToProcess, int startAtMillisecond);

        IWithConfiguration On(float[] audioSamples, int millisecondsToProcess, int startAtMillisecond);
    }
}