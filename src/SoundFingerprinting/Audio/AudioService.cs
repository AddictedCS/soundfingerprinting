namespace SoundFingerprinting.Audio
{
    using System.Collections.Generic;

    /// <inheritdoc cref="IAudioService"/>
    public abstract class AudioService : IAudioService
    {
        /// <inheritdoc cref="IAudioService.SupportedFormats"/>
        public abstract IReadOnlyCollection<string> SupportedFormats { get; }
        
        /// <inheritdoc cref="IAudioService.GetLengthInSeconds"/>
        public abstract float GetLengthInSeconds(string file);

        /// <inheritdoc cref="IAudioService.ReadMonoSamplesFromFile(string,int,double,double)"/>
        public abstract AudioSamples ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate, double seconds, double startAt);

        /// <inheritdoc cref="IAudioService.ReadMonoSamplesFromFile(string,int)"/>
        public AudioSamples ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate)
        {
            return ReadMonoSamplesFromFile(pathToSourceFile, sampleRate, 0, 0);
        }
    }
}