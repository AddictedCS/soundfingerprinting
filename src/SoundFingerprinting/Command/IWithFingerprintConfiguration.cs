namespace SoundFingerprinting.Command
{
    using System;

    using SoundFingerprinting.Configuration;

    /// <summary>
    ///  Contract identifying fingerprinting configuration parameters.
    /// </summary>
    public interface IWithFingerprintConfiguration : IInterceptFingerprints
    {
        /// <summary>
        ///  Set fingerprinting configuration object to use for fingerprinting.
        /// </summary>
        /// <param name="configuration">Instance of the <see cref="AVFingerprintConfiguration"/> class.</param>
        /// <returns>Instance of an <see cref="IInterceptFingerprints"/> interface for services selector.</returns>
        /// <remarks>
        ///  Use the same configuration parameters for fingerprinting and querying. <br />
        ///  By default <see cref="DefaultAVFingerprintConfiguration"/> instance is used, no need to specify it explicitly.
        /// </remarks>
        IInterceptFingerprints WithFingerprintConfig(AVFingerprintConfiguration configuration);

        /// <summary>
        ///  Set fingerprinting configuration object to use for fingerprinting.
        /// </summary>
        /// <param name="amendFunctor">Functor to amend default fingerprinting configuration.</param>
        /// <returns>Instance of an <see cref="IInterceptFingerprints"/> interface for services selector.</returns>
        /// <remarks>
        ///  Use the same configuration parameters for fingerprinting and querying.
        /// </remarks>
        IInterceptFingerprints WithFingerprintConfig(Func<AVFingerprintConfiguration, AVFingerprintConfiguration> amendFunctor);
    }
}