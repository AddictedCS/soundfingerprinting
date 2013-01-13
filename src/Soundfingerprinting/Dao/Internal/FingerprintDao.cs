namespace Soundfingerprinting.Dao.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Soundfingerprinting.DbStorage.Entities;

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
                                .WithParametersFromModel(fingerprint)
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
                    .AsListOfModel<Fingerprint>();
        }

        public IList<Fingerprint> ReadFingerprintsByTrackId(int trackId, int numberOfFingerprintsToRead)
        {
            return PrepareStoredProcedure(SpReadFingerprintByTrackId)
                    .WithParameter("Id", trackId)
                    .WithParameter("NumberOfFingerprintsToRead", numberOfFingerprintsToRead)
                    .Execute()
                    .AsListOfModel<Fingerprint>();
        }

        public Fingerprint ReadById(int id)
        {
            return PrepareStoredProcedure(SpReadFingerprintById)
                    .WithParameter("Id", id)
                    .Execute()
                    .AsModel<Fingerprint>();
        }

        public IList<Fingerprint> ReadById(IEnumerable<int> ids)
        {
            return ids.Select(ReadById).ToList();
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
                        .AsListOfModel<Fingerprint>();

                result.Add(track.Id, fingerprints);
            }

            return result;
        }
    }
}