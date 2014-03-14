namespace SoundFingerprinting.Command
{
    using System;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;

    public interface IWithFingerprintConfiguration
    {
        IUsingFingerprintServices WithFingerprintConfig(IFingerprintConfiguration configuration);

        IUsingFingerprintServices WithFingerprintConfig<T>() where T : IFingerprintConfiguration, new();

        IUsingFingerprintServices WithFingerprintConfig(Action<CustomFingerprintConfiguration> functor);

        IUsingFingerprintServices WithDefaultFingerprintConfig();
    }

    public interface IUsingFingerprintServices
    {
        IFingerprintCommand UsingServices(FingerprintServices services);

        IFingerprintCommand UsingServices(Action<FingerprintServices> services);
    }

    public class FingerprintServices
    {
        public IAudioService AudioService { get; set; }
    }
}