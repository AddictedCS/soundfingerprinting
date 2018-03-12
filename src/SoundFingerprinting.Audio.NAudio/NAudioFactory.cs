namespace SoundFingerprinting.Audio.NAudio
{
    using global::NAudio.MediaFoundation;
    using global::NAudio.Wave;

    internal class NAudioFactory : INAudioFactory
    {
        public WaveStream GetStream(string pathToAudioFile)
        {
            // This class assumess media foundation libraries are installed on target machine
            // In case you are running on Azure (Windows Server 2012) install Server Media Foundation feature
            return new MediaFoundationReader(pathToAudioFile);
        }

        public WaveFileWriter GetWriter(string pathToFile, int sampleRate, int numberOfChannels)
        {
            return new WaveFileWriter(pathToFile, WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, numberOfChannels));
        }

        public WaveFormat GetWaveFormat(int sampleRate, int numberOfChannels)
        {
            return WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, numberOfChannels);
        }

        public MediaFoundationTransform GetResampler(WaveStream streamToResample, int sampleRate, int numberOfChannels, int resamplerQuality)
        {
            return new MediaFoundationResampler(streamToResample, GetWaveFormat(sampleRate, numberOfChannels))
                {
                    ResamplerQuality = resamplerQuality
                };
        }

        public WaveInEvent GetWaveInEvent(int sampleRate, int numberOfChannels)
        {
            return new WaveInEvent
                {
                    WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, numberOfChannels)
                };
        }

        public void CreateWaveFile(string pathToWaveFile, IWaveProvider waveProvider)
        {
            WaveFileWriter.CreateWaveFile(pathToWaveFile, waveProvider);
        }
    }
}
