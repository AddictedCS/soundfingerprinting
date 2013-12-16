namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.Query.Configuration;

    public interface IWithQueryConfiguration
    {
        IFingerprintQueryCommand WithQueryConfiguration(IQueryConfiguration queryConfiguration);
    }
}