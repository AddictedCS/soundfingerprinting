namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder.Internal
{
    using Soundfingerprinting.Fingerprinting.Configuration;

    internal class WorkUnitParameterObject : IWorkUnitParameterObject
    {
        public string PathToAudioFile { get; set; }

        public float[] AudioSamples { get; set; }

        public IFingerprintingConfiguration FingerprintingConfiguration { get; set; }

        public int StartAtMilliseconds { get; set; }

        public int MillisecondsToProcess { get; set; }
    }
}