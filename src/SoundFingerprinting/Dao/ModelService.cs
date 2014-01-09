namespace SoundFingerprinting.Dao
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Dao.Internal;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    public class ModelService : IModelService
    {
        private readonly TrackDao trackDao;

        private readonly HashBinMinHashDao hashBinMinHashDao;

        private readonly SubFingerprintDao subFingerprintDao;

        private readonly PermutationsDao permutationsDao;

        public ModelService()
            : this(DependencyResolver.Current.Get<IDatabaseProviderFactory>(), DependencyResolver.Current.Get<IModelBinderFactory>())
        {
        }

        public ModelService(IDatabaseProviderFactory databaseProviderFactory, IModelBinderFactory modelBinderFactory)
        {
            trackDao = new TrackDao(databaseProviderFactory, modelBinderFactory);
            hashBinMinHashDao = new HashBinMinHashDao(databaseProviderFactory, modelBinderFactory);
            subFingerprintDao = new SubFingerprintDao(databaseProviderFactory, modelBinderFactory);
            permutationsDao = new PermutationsDao(databaseProviderFactory, modelBinderFactory);
        }

        public IList<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] buckets, int threshold)
        {
            var subFingerprints = hashBinMinHashDao.ReadSubFingerprintDataByHashBucketsWithThreshold(buckets, threshold);
            var fingerprints = subFingerprints as IList<SubFingerprint> ?? subFingerprints.ToList();
            if (subFingerprints != null && fingerprints.Any())
            {
                return fingerprints.Select(subFingerprint => new SubFingerprintData
                                                                 {
                                                                     Signature = subFingerprint.Signature, 
                                                                     TrackReference = new RDBMSTrackReference(subFingerprint.TrackId), 
                                                                     SubFingerprintReference = new RDBMSSubFingerprintReference(subFingerprint.Id)
                                                                 })
                                                                 .ToList();
            }

            return Enumerable.Empty<SubFingerprintData>().ToList();
        }
    
        public int[][] ReadPermutationsForLSHAlgorithm()
        {
            return permutationsDao.ReadPermutationsForLSHAlgorithm();
        }

        public ITrackReference InsertTrack(TrackData track)
        {
            return new RDBMSTrackReference(trackDao.Insert(GetTrackFromTrackData(track)));
        }

        
        public void InsertHashDataForTrack(byte[] subFingerprintSignature, long[] hashBuckets, ITrackReference trackReference)
        {
            subFingerprintDao.Insert()
        }

        public IList<TrackData> ReadAllTracks()
        {
            throw new NotImplementedException();
        }

        public IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title)
        {
            throw new NotImplementedException();
        }

        public TrackData ReadTrackByReference(ITrackReference trackReference)
        {
            throw new NotImplementedException();
        }

        public TrackData ReadTrackByISRC(string isrc)
        {
            throw new NotImplementedException();
        }

        public int DeleteTrack(ITrackReference trackReference)
        {
            throw new NotImplementedException();
        }

        private static Track GetTrackFromTrackData(TrackData track)
        {
            return new Track(track.ISRC, track.Artist, track.Title, track.Album, track.ReleaseYear, track.TrackLengthSec);
        }
    }
}
