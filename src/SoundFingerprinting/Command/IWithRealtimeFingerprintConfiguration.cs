namespace SoundFingerprinting.Command
{
    using System;
    using SoundFingerprinting.Configuration;

    /// <summary>
    ///  Contract for fingerprint configuration selection of the realtime fingerprint command.
    /// </summary>
    public interface IWithRealtimeFingerprintConfiguration : IInterceptRealtimeFingerprints
    {
        /// <summary>
        ///  Sets the fingerprint configuration.
        /// </summary>
        /// <param name="configuration">Instance of the <see cref="AVFingerprintConfiguration"/> class.</param>
        /// <returns>Interceptors selector.</returns>
        /// <remarks>
        ///  Use the same configuration parameters for fingerprinting and querying. To create query hashes from a realtime source, pass <see cref="AVQueryConfiguration.FingerprintConfiguration"/>. <br />
        ///  By default <see cref="DefaultAVFingerprintConfiguration"/> is used.
        /// </remarks>
        IInterceptRealtimeFingerprints WithFingerprintConfig(AVFingerprintConfiguration configuration);

        /// <summary>
        ///  Amends the default fingerprint configuration.
        /// </summary>
        /// <param name="amendFunctor">Functor to amend the default configuration.</param>
        /// <returns>Interceptors selector.</returns>
        IInterceptRealtimeFingerprints WithFingerprintConfig(Func<AVFingerprintConfiguration, AVFingerprintConfiguration> amendFunctor);
    }
}
