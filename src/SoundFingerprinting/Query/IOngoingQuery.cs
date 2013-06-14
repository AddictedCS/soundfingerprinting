namespace SoundFingerprinting.Query
{
    public interface IOngoingQuery
    {
        IOngoingQueryConfigurationWithFingerprinting From(string pathToAudioFile);

        IOngoingQueryConfigurationWithFingerprinting From(string pathToAudioFile, int secondsToProcess, int startAtSecond);

        IOngoingQueryConfigurationWithFingerprinting From(float[] audioSamples);

        IOngoingQueryConfiguration From(bool[] fingerprint);
    }
}