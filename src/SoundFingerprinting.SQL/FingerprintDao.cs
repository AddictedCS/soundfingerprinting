namespace SoundFingerprinting.SQL
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.SQL.Connection;
    using SoundFingerprinting.SQL.ORM;

    internal class FingerprintDao : AbstractDao, IFingerprintDao
    {
        private const string SpInsertFingerprint = "sp_InsertFingerprint";
        private const string SpReadFingerprintByTrackId = "sp_ReadFingerprintByTrackId";

        public FingerprintDao()
            : base(
                DependencyResolver.Current.Get<IDatabaseProviderFactory>(),
                DependencyResolver.Current.Get<IModelBinderFactory>())
        {
            // no op
        }

        public FingerprintDao(IDatabaseProviderFactory databaseProvider, IModelBinderFactory modelBinderFactory)
            : base(databaseProvider, modelBinderFactory)
        {
        }

        public IModelReference InsertFingerprint(FingerprintData fingerprint)
        {
            byte[] byteSignature = GetByteArrayFromBool(fingerprint.Signature);
            var fingerprintId = PrepareStoredProcedure(SpInsertFingerprint)
                                .WithParameter("Signature", byteSignature)
                                .WithParameter("TrackId", fingerprint.TrackReference.Id, DbType.Int32)
                                .Execute()
                                .AsScalar<int>();

            return fingerprint.FingerprintReference = new ModelReference<int>(fingerprintId);
        }

        public IList<FingerprintData> ReadFingerprintsByTrackReference(IModelReference trackReference)
        {
            return PrepareStoredProcedure(SpReadFingerprintByTrackId)
                    .WithParameter("TrackId", trackReference.Id, DbType.Int32)
                    .Execute()
                    .AsList(reader =>
                        {
                            int fingerprintId = reader.GetInt32("Id");
                            int dbTrackId = reader.GetInt32("TrackId");
                            byte[] byteSignature = (byte[])reader.GetRaw("Signature");
                            return new FingerprintData
                                {
                                    Signature = GetBoolArrayFromByte(byteSignature),
                                    FingerprintReference = new ModelReference<int>(fingerprintId),
                                    TrackReference = new ModelReference<int>(dbTrackId)
                                };
                        });
        }

        private static byte[] GetByteArrayFromBool(bool[] bools)
        {
            BitArray bits = new BitArray(bools);
            byte[] bytes = new byte[bools.Length / 8];
            bits.CopyTo(bytes, 0);
            return bytes;
        }

        private static bool[] GetBoolArrayFromByte(byte[] bytes)
        {
            bool[] bools = new bool[bytes.Length * 8];
            BitArray bits = new BitArray(bytes);
            bits.CopyTo(bools, 0);
            return bools;
        }
    }
}