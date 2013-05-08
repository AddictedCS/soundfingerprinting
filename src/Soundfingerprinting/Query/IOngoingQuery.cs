namespace Soundfingerprinting.Query
{
    public interface IOngoingQuery
    {
        IOngoingQueryConfigurationWithFingerprinting From(string pathToAudioFile);

        IOngoingQueryConfigurationWithFingerprinting From(string pathToAudioFile, int millisecondsToProcess, int startAtMillisecond);

        IOngoingQueryConfigurationWithFingerprinting From(float[] audioSamples);

        IOngoingQueryConfiguration From(bool[] fingerprint);
    }
}