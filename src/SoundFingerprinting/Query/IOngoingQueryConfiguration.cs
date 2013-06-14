namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.Query.Configuration;

    public interface IOngoingQueryConfiguration
    {
        IFingerprintingQueryUnit With(IQueryConfiguration queryConfiguration);
    }
}