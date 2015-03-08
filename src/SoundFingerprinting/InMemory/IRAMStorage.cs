namespace SoundFingerprinting.InMemory
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    internal interface IRAMStorage
    {
        IDictionary<IModelReference, SubFingerprintData> SubFingerprints { get; }       // key: sub fingerprint reference

        IDictionary<IModelReference, TrackData> Tracks { get; }                         // key: track reference

        IDictionary<IModelReference, IDictionary<IModelReference, HashedFingerprint>> TracksHashes { get; } // key: track reference, value: key - sub-fingerprint-id

        IDictionary<IModelReference, List<FingerprintData>> Fingerprints { get; }       // key: track reference

        IDictionary<long, List<IModelReference>>[] HashTables { get; }                  // value: list of sub-fingerprints

        int NumberOfHashTables { get; }

        void Reset(int numberOfHashTables);
    }
}