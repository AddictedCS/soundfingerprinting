namespace SoundFingerprinting.Audio.Bass
{
    internal interface IBassStreamFactory
    {
        int CreateStream(string pathToFile);

        int CreateMixerStream(int sampleRate);

        int CreateStreamFromStreamingUrl(string streamingUrl);

        int CreateStreamFromMicrophone(int sampleRate);
    }
}