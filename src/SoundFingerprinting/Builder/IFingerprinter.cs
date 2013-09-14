namespace SoundFingerprinting.Builder
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using SoundFingerprinting.Dao.Entities;

    public interface IFingerprinter
    {
        Task<List<bool[]>> AsIs();

        Task<List<bool[]>> AsIs(CancellationToken token);

        Task<List<Fingerprint>> ForTrack(int trackId);

        Task<List<Fingerprint>> ForTrack(int trackId, CancellationToken token);

        IHasher HashIt();
    }
}
