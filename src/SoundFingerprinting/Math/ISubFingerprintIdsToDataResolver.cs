namespace SoundFingerprinting.Math
{
    using System.Collections.Generic;
    using SoundFingerprinting.DAO.Data;

    public interface ISubFingerprintIdsToDataResolver
    {
        IEnumerable<SubFingerprintData> ResolveFromIds(IEnumerable<uint> ids, ISet<string> clusters);
    }
}