namespace SoundFingerprinting.Audio.NAudio.Play
{
    using global::NAudio.Wave;

    internal interface INAudioPlayAudioFactory
    {
        IWavePlayer CreateNewWavePlayer();

        WaveStream CreateNewStreamFromFilename(string fileName);
    }
}