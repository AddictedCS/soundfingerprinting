namespace SoundFingerprinting.Builder
{
    public interface ITargetOn
    {
        IWithFingerprintConfiguration On(string pathToAudioFile);

        IWithFingerprintConfiguration On(float[] audioSamples);

        IWithFingerprintConfiguration On(string pathToAudioFile, int secondsToProcess, int startAtSecond);
    }
}