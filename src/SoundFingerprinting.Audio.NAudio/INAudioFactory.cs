namespace SoundFingerprinting.Audio.NAudio
{
    using global::NAudio.MediaFoundation;
    using global::NAudio.Wave;

    internal interface INAudioFactory
    {
        WaveStream GetStream(string pathToAudioFile);

        WaveFileWriter GetWriter(string pathToFile, int sampleRate, int numberOfChannels);

        WaveFormat GetWaveFormat(int sampleRate, int numberOfChannels);

        MediaFoundationTransform GetResampler(WaveStream streamToResample, int sampleRate, int numberOfChannels, int resamplerQuality);

        WaveInEvent GetWaveInEvent(int sampleRate, int numberOfChannels);

        void CreateWaveFile(string pathToWaveFile, IWaveProvider waveProvider);
    }
}
