namespace SoundFingerprinting.Audio.Bass
{
    using System.Collections.Generic;

    using Un4seen.Bass;

    public interface IBassServiceProxy
    {
        void RegisterBass(string email, string registrationKey);

        bool LoadMe(string targetPath);

        int GetVersion();

        int GetMixerVersion();

        int GetFxVersion();

        IDictionary<int, string> PluginLoadDirectory(string path);

        bool Init(int sampleRate);

        bool SetDefaultConfigs();

        string GetLastError();

        int StreamCreateFile(string pathToFile);

        int GetRecordingDevice();

        int CreateStream(string pathToAudioFile, BASSFlag flags);

        int CreateMixerStream(int sampleRate, int channels, BASSFlag flags);

        bool CombineMixerStreams(int mixerStream, int stream, BASSFlag flags);

        bool ChannelSetPosition(int stream, int seekToSecond);

        int ChannelGetData(int stream, float[] buffer, int lengthInBytes);
    }

    internal class BassServiceProxy
    {
    }
}
