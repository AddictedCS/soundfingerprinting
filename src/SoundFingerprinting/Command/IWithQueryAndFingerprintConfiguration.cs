namespace SoundFingerprinting.Command
{
    using System;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;

    public interface IWithQueryAndFingerprintConfiguration
    {
        IUsingQueryServices WithConfigs(IFingerprintConfiguration fingerprintConfiguration, IQueryConfiguration queryConfiguration);

        IUsingQueryServices WithConfigs<T1, T2>() where T1 : IFingerprintConfiguration, new() where T2 : IQueryConfiguration, new();

        IUsingQueryServices WithConfigs(Action<CustomFingerprintConfiguration> fingerprintConfiguration, Action<CustomQueryConfiguration> queryConfiguration);

        IUsingQueryServices WithDefaultConfigs();
    }

     public interface IUsingQueryServices
     {
         IQueryCommand UsingServices(QueryServices services);

         IQueryCommand UsingServices(Action<QueryServices> services);
     }

    public class QueryServices
    {
        public IModelService ModelService { get; set; }

        public IAudioService AudioService { get; set; }
    }
}