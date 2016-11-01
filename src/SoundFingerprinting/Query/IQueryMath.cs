namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.Configuration;

    internal interface IQueryMath
    {
        double AdjustSnippetLengthToConfigsUsedDuringFingerprinting(double snipetLength, FingerprintConfiguration fingerprintConfiguration);
    }
}