namespace SoundFingerprinting.Dao.InMemory
{
    using System;
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    public class InMemoryModelService : IModelService
    {
        private readonly TrackStorageDao trackStorageDao;
        private readonly FingerprintStorageDao fingerprintStorageDao;
        private readonly SubFingerprintStorageDao subFingerprintStorageDao;
        private readonly HashBinStorageDao hashBinStorageDao;

        public InMemoryModelService()
        {
            trackStorageDao = new TrackStorageDao();
            fingerprintStorageDao = new FingerprintStorageDao();
            subFingerprintStorageDao = new SubFingerprintStorageDao();
            hashBinStorageDao = new HashBinStorageDao();
        }

        public IModelReference InsertFingerprint(FingerprintData fingerprintData)
        {
            if (!(fingerprintData.TrackReference is ModelReference<int>))
            {
                throw new NotSupportedException("Cannot insert this type to in memory structure");
            }

            int fingerprintId = fingerprintStorageDao.Insert(
                fingerprintData.Signature, ((ModelReference<int>)fingerprintData.TrackReference).Id);
            return fingerprintData.FingerprintReference = new ModelReference<int>(fingerprintId);
        }

        public IModelReference InsertTrack(TrackData track)
        {
            trackStorageDao.Insert(track);
            return track.TrackReference;
        }

        public void InsertHashDataForTrack(IEnumerable<HashData> hashes, IModelReference trackReference)
        {
            if (!(trackReference is ModelReference<int>))
            {
                throw new NotSupportedException();
            }

            foreach (var hashData in hashes)
            {
                long subFingerprintId = subFingerprintStorageDao.Insert(
                    hashData.SubFingerprint, ((ModelReference<int>)trackReference).Id);
                hashBinStorageDao.Insert(hashData.HashBins, subFingerprintId);
            }
        }

        public IList<TrackData> ReadAllTracks()
        {
            return trackStorageDao.ReadAll();
        }

        public IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title)
        {
            return trackStorageDao.ReadTrackByArtistAndTitleName(artist, title);
        }

        public IList<FingerprintData> ReadFingerprintsByTrackReference(IModelReference trackReference)
        {
            if (!(trackReference is ModelReference<int>))
            {
                throw new NotSupportedException("Cannot perform this operation");
            }

            return fingerprintStorageDao.ReadFingerprintsByTrackId(((ModelReference<int>)trackReference).Id);
        }

        public TrackData ReadTrackByReference(IModelReference trackReference)
        {
            if (!(trackReference is ModelReference<int>))
            {
                throw new NotSupportedException("Cannot read from the underlying source with specified track reference");
            }

            return trackStorageDao.ReadById(((ModelReference<int>)trackReference).Id);
        }

        public TrackData ReadTrackByISRC(string isrc)
        {
            return trackStorageDao.ReadTrackByISRC(isrc);
        }

        public int DeleteTrack(IModelReference trackReference)
        {
            if (!(trackReference is ModelReference<int>))
            {
                throw new NotSupportedException();
            }

            return trackStorageDao.DeleteTrack(((ModelReference<int>)trackReference).Id);
        }

        public IList<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] buckets, int threshold)
        {
            throw new NotImplementedException();
        }
    }
}
