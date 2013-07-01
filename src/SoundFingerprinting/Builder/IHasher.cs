namespace SoundFingerprinting.Builder
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using SoundFingerprinting.Dao.Entities;

    public interface IHasher
    {
        Task<List<byte[]>> AsIs();

        Task<List<byte[]>> AsIs(CancellationToken token);

        Task<List<SubFingerprint>> ForTrack(int trackId);

        Task<List<SubFingerprint>> ForTrack(int trackId, CancellationToken token);
    }
}