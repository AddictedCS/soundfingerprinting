namespace Soundfingerprinting.Fingerprinting.FingerprintUnitBuilder
{
    public interface ITargetOn
    {
        IWithConfiguration On(string pathToAudioFile);

        IWithConfiguration On(float[] audioSamples);

        IWithConfiguration On(string pathToAudioFile, int millisecondsToProcess, int startAtMillisecond);
    }
}