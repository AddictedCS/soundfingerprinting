namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder
{
    using Soundfingerprinting.Fingerprinting.Configuration;

    public interface IWorkUnitParameterObject
    {
        string PathToAudioFile { get; }

        float[] AudioSamples { get; }

        IFingerprintingConfig FingerprintingConfig { get; }

        int StartAtMilliseconds { get; }

        int MillisecondsToProcess { get; }
    }
}