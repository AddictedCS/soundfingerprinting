namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder
{
    using Soundfingerprinting.Fingerprinting.Configuration;

    public class WorkUnitParameterObject
    {
        public string PathToAudioFile { get; set; }

        public float[] AudioSamples { get; set; }

        public IFingerprintingConfiguration FingerprintingConfiguration { get; set; }

        public int StartAtMilliseconds { get; set; }

        public int MillisecondsToProcess { get; set; }
    }
}