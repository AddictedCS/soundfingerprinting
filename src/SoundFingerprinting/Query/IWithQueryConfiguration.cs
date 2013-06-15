namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.Query.Configuration;

    public interface IWithQueryConfiguration
    {
        IFingerprintQueryUnit With(IQueryConfiguration queryConfiguration);
    }
}