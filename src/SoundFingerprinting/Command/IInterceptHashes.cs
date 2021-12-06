namespace SoundFingerprinting.Command
{
    using System;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Contract for hashes interception.
    /// </summary>
    public interface IInterceptHashes : IUsingQueryServices
    {
        /// <summary>
        ///  Intercept query hashes before they are used to query the data source.
        /// </summary>
        /// <param name="hashes">Hashes interceptor that are used to query the storage.</param>
        /// <returns>Query services selector.</returns>
        IUsingQueryServices Intercept(Func<AVHashes, AVHashes> hashes); 
    }
}