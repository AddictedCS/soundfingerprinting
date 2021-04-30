namespace SoundFingerprinting.Command
{
    using System;
    using SoundFingerprinting.Data;

    public interface IInterceptRealtimeHashes : IUsingRealtimeQueryServices
    {
        /// <summary>
        ///  Intercept query hashes to enhance the payload.
        /// </summary>
        /// <param name="hashes">Hashes interceptor that are used to query the storage.</param>
        /// <returns>Query services selector.</returns>
        IUsingRealtimeQueryServices Intercept(Func<Hashes, Hashes> hashes);
    }
}