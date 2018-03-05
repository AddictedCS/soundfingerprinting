using System;
using System.Collections.Generic;

namespace SoundFingerprinting.Audio
{
    public class SoundFingerprintingAudioService : AudioService
    {
        public override AudioSamples ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate, double seconds, double startAt)
        {
            var format = WaveFormat.FromFile(pathToSourceFile);

            if(format.SampleRate < 5512)
                throw new ArgumentException($"Supplied file format sample rate is less than accepted range [5512, 44100]");
            if(format.BitsPerSample < 8)
                throw new ArgumentException($"Supplied file format bits per sample is less than accepted range [8, 32]");
            if(format.Channels < 0 || format.Channels > 2)
                throw new ArgumentException($"Supplied file format channels is not in the accepted range [1, 2]");

            return null;
        }

        public override float GetLengthInSeconds(string pathToSourceFile)
        {
            throw new NotImplementedException();
        }

        public override IReadOnlyCollection<string> SupportedFormats
        {
            get
            {
                return new[] { ".wav" };
            }
        }
    }
}
