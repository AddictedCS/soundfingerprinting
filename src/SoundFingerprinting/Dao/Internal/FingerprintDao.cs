namespace SoundFingerprinting.Dao.Internal
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Dao.Entities;
    using SoundFingerprinting.Dao.Utils;

    internal class FingerprintDao : AbstractDao
    {
        private const string SpInsertFingerprint = "sp_InsertFingerprint";
        private const string SpReadFingerprints = "sp_ReadFingerprints";
        private const string SpReadFingerprintByTrackId = "sp_ReadFingerprintByTrackId";
        private const string SpReadFingerprintById = "sp_ReadFingerprintById";

        public FingerprintDao(IDatabaseProviderFactory databaseProvider, IModelBinderFactory modelBinderFactory)
            : base(databaseProvider, modelBinderFactory)
        {
        }

        public void Insert(Fingerprint fingerprint)
        {
            fingerprint.Id = PrepareStoredProcedure(SpInsertFingerprint)
                                .WithParametersFromModel(new FingerprintDto(fingerprint))
                                .Execute()
                                .AsScalar<int>();
        }

        public void Insert(IEnumerable<Fingerprint> collection)
        {
            foreach (var fingerprint in collection)
            {
                Insert(fingerprint);
            }
        }

        public IList<Fingerprint> Read()
        {
            return PrepareStoredProcedure(SpReadFingerprints)
                    .Execute()
                    .AsListOfModel<FingerprintDto>()
                    .Select(fingerprintDto => fingerprintDto.ToFingerprint())
                    .ToList();
        }

        public IList<Fingerprint> ReadFingerprintsByTrackId(int trackId, int numberOfFingerprintsToRead)
        {
            return PrepareStoredProcedure(SpReadFingerprintByTrackId)
                    .WithParameter("Id", trackId)
                    .WithParameter("NumberOfFingerprintsToRead", numberOfFingerprintsToRead)
                    .Execute()
                    .AsListOfModel<FingerprintDto>()
                    .Select(fingerprintDto => fingerprintDto.ToFingerprint())
                    .ToList();
        }

        public Fingerprint ReadById(int id)
        {
            FingerprintDto fingerprintDto = PrepareStoredProcedure(SpReadFingerprintById)
                    .WithParameter("Id", id)
                    .Execute()
                    .AsModel<FingerprintDto>();

            return fingerprintDto != null ? fingerprintDto.ToFingerprint() : null;
        }

        public IList<Fingerprint> ReadById(IEnumerable<int> ids)
        {
            return ids.Select(ReadById).Where(fingerprint => fingerprint != null).ToList();
        }

        public IDictionary<int, IList<Fingerprint>> ReadFingerprintsByMultipleTrackId(IEnumerable<Track> tracks, int numberOfFingerprintsToRead)
        {
            var result = new Dictionary<int, IList<Fingerprint>>();
            foreach (var track in tracks)
            {
                IList<Fingerprint> fingerprints =
                    PrepareStoredProcedure(SpReadFingerprintByTrackId)
                        .WithParameter("Id", track.Id)
                        .WithParameter("NumberOfFingerprintsToRead", numberOfFingerprintsToRead)
                        .Execute()
                        .AsListOfModel<FingerprintDto>()
                        .Select(fingerprintDto => fingerprintDto.ToFingerprint())
                        .Where(fingerprint => fingerprint != null)
                        .ToList();

                if (fingerprints.Count == 0)
                {
                    continue;
                }

                if (result.ContainsKey(track.Id))
                {
                    foreach (var fingerprint in fingerprints)
                    {
                        result[track.Id].Add(fingerprint);
                    }
                }
                else
                {
                    result.Add(track.Id, fingerprints);
                }
            }

            return result;
        }

        private class FingerprintDto
        {
            public FingerprintDto()
            {
            }

            public FingerprintDto(Fingerprint fingerprint)
            {
                Id = fingerprint.Id;
                TrackId = fingerprint.TrackId;
                TotalFingerprintsPerTrack = fingerprint.TotalFingerprintsPerTrack;
                SongOrder = fingerprint.SongOrder;
                Signature = ArrayUtils.GetByteArrayFromBool(fingerprint.Signature);
            }

            public int Id { get; set; }

            public int TrackId { get; set; }

            public int TotalFingerprintsPerTrack { get; set; }

            public int SongOrder { get; set; }

            public byte[] Signature { get; set; }

            public Fingerprint ToFingerprint()
            {
                return new Fingerprint(ArrayUtils.GetBoolArrayFromByte(Signature), TrackId, SongOrder, TotalFingerprintsPerTrack) { Id = Id };
            }
        }
    }
}