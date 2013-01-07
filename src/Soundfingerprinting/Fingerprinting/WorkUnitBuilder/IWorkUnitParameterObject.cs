namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder
{
    using Soundfingerprinting.Fingerprinting.Configuration;

    public interface IWorkUnitParameterObject
    {
        string PathToAudioFile { get; }

        float[] AudioSamples { get; }

        IFingerprintingConfiguration FingerprintingConfiguration { get; }

        int StartAtMilliseconds { get; }

        int MillisecondsToProcess { get; }
    }
}