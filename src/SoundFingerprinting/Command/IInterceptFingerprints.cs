namespace SoundFingerprinting.Command;

using System;
using SoundFingerprinting.Data;

/// <summary>
///  Intercept original fingerprints
/// </summary>
public interface IInterceptFingerprints : IUsingFingerprintServices
{
    /// <summary>
    ///  Intercept original fingerprints. <br/>
    ///  A typical framework user would never need to access underlying fingerprint data, though it may be helpful for research purposes.
    /// </summary>
    /// <param name="fingerprintsInterceptor">Original fingerprints.</param>
    /// <returns>Fingerprints services selector.</returns>
    IUsingFingerprintServices Intercept(Action<AVFingerprints> fingerprintsInterceptor);
}