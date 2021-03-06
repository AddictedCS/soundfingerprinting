﻿namespace SoundFingerprinting.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Math;

    public class InMemoryModelService : IAdvancedModelService
    {
        private readonly IRAMStorage ramStorage;
        private readonly UIntModelReferenceTracker modelReferenceTracker;
        private readonly IModelReferenceProvider spectralReferenceProvider;

        public InMemoryModelService() : this(new RAMStorage(25), new StandardGroupingCounter())
        {
        }

        public InMemoryModelService(string loadFrom) : this(new RAMStorage(25, loadFrom), new StandardGroupingCounter())
        {
        }

        public InMemoryModelService(IRAMStorage ramStorage, IGroupingCounter groupingCounter): this(new TrackDao(ramStorage), new SubFingerprintDao(ramStorage, groupingCounter), new SpectralImageDao(ramStorage), ramStorage)
        {
        }

        private InMemoryModelService(ITrackDao trackDao, ISubFingerprintDao subFingerprintDao, ISpectralImageDao spectralImageDao, IRAMStorage ramStorage)
        {
            this.ramStorage = ramStorage;
            Id = "in-memory-model-service";
            TrackDao = trackDao;
            SubFingerprintDao = subFingerprintDao;
            SpectralImageDao = spectralImageDao;

            IModelReference? lastTrackReference = null;
            uint maxTrackId = 0;
            if (ramStorage.Tracks.Any())
            {
                (lastTrackReference, maxTrackId) = ramStorage.Tracks.Keys
                    .Select(_ => (_, _.Get<uint>()))
                    .OrderByDescending(_ => _.Item2)
                    .First();
            }
 
            uint maxSubFingerprintId = 0;
            if (lastTrackReference != null)
            {
                maxSubFingerprintId = ramStorage
                    .ReadSubFingerprintByTrackReference(lastTrackReference)
                    .Max(_ => _.SubFingerprintReference.Get<uint>());
            }
            
            modelReferenceTracker = new UIntModelReferenceTracker(maxTrackId, maxSubFingerprintId);

            uint maxSpectralImageId = 0;
            if (lastTrackReference != null)
            {
                var spectralImages =  ramStorage.GetSpectralImagesByTrackReference(lastTrackReference).ToList();
                if (spectralImages.Any())
                {
                    maxSpectralImageId = spectralImages.Max(_ => _.SpectralImageReference.Get<uint>());
                }
            }

            spectralReferenceProvider = new UIntModelReferenceProvider(maxSpectralImageId);
        }

        public void Snapshot(string path)
        {
            ramStorage.Snapshot(path);
        }
        
        private string Id { get; }
        
        private ITrackDao TrackDao { get; }
        
        private ISubFingerprintDao SubFingerprintDao { get; }
        
        private ISpectralImageDao SpectralImageDao { get; }
        
        public IEnumerable<ModelServiceInfo> Info => new[] { new ModelServiceInfo(Id, TrackDao.Count, SubFingerprintDao.SubFingerprintsCount, SubFingerprintDao.HashCountsPerTable.ToArray()) };

        /// <inheritdoc cref="IModelService.Insert"/>
        public void Insert(TrackInfo track, Hashes hashes)
        {
            var fingerprints = hashes.ToList();
            if (!fingerprints.Any())
            {
                return;
            }

            var (trackData, subFingerprints) = modelReferenceTracker.AssignModelReferences(track, hashes);
            TrackDao.InsertTrack(trackData);
            SubFingerprintDao.InsertSubFingerprints(subFingerprints);
        }

        /// <inheritdoc cref="IModelService.UpdateTrack"/>
        public void UpdateTrack(TrackInfo trackInfo)
        {
            var track = TrackDao.ReadTrackById(trackInfo.Id);
            if (track == null)
            {
                throw new ArgumentException($"Could not find track {trackInfo.Id} to update", nameof(trackInfo.Id));
            }

            if (trackInfo.MediaType != track.MediaType)
            {
                throw new ArgumentException($"Can't update media type from {trackInfo.MediaType} to {track.MediaType}. Delete {track.Id} and reinsert with new media type.");
            }

            var subFingerprints = SubFingerprintDao.ReadHashedFingerprintsByTrackReference(track.TrackReference);
            var hashes = new Hashes(subFingerprints.Select(subFingerprint => new HashedFingerprint(subFingerprint.Hashes, subFingerprint.SequenceNumber, subFingerprint.SequenceAt, subFingerprint.OriginalPoint)), track.Length, track.MediaType);
            DeleteTrack(trackInfo.Id);
            Insert(trackInfo, hashes);
        }

        /// <inheritdoc cref="IModelService.Query"/>
        public IEnumerable<SubFingerprintData> Query(Hashes hashes, QueryConfiguration config)
        {
            var queryHashes = hashes.Select(_ => _.HashBins).ToList();
            return queryHashes.Any() ? SubFingerprintDao.ReadSubFingerprints(queryHashes, config) : Enumerable.Empty<SubFingerprintData>();
        }

        /// <inheritdoc cref="IModelService.ReadHashesByTrackId"/>
        public AVHashes ReadHashesByTrackId(string trackId)
        {
            var track = TrackDao.ReadTrackById(trackId);
            if (track == null)
            {
                return AVHashes.Empty;
            }

            var fingerprints = SubFingerprintDao
                .ReadHashedFingerprintsByTrackReference(track.TrackReference)
                .Select(ToHashedFingerprint);
            return new AVHashes(new Hashes(fingerprints, track.Length, MediaType.Audio), Hashes.GetEmpty(MediaType.Video));
        }

        /// <inheritdoc cref="IModelService.GetTrackIds"/>
        public IEnumerable<string> GetTrackIds()
        {
            return TrackDao.GetTrackIds();
        }

        /// <inheritdoc cref="IModelService.ReadTracksByReferences"/>
        public IEnumerable<TrackData> ReadTracksByReferences(IEnumerable<IModelReference> references)
        {
            return TrackDao.ReadTracksByReferences(references);
        }

        /// <inheritdoc cref="IModelService.ReadTrackById"/>
        public TrackInfo? ReadTrackById(string trackId)
        {
            var trackData = TrackDao.ReadTrackById(trackId);
            if (trackData == null)
            {
                return null;
            }

            var metaFields = CopyMetaFields(trackData.MetaFields);
            metaFields.Add("TrackLength", $"{trackData.Length: 0.000}");
            return new TrackInfo(trackData.Id, trackData.Title, trackData.Artist, metaFields, trackData.MediaType);
        }

        /// <inheritdoc cref="IModelService.DeleteTrack"/>
        public void DeleteTrack(string trackId)
        {
            var track = TrackDao.ReadTrackById(trackId);
            if (track == null)
            {
                return;
            }

            var trackReference = track.TrackReference;
            SubFingerprintDao.DeleteSubFingerprintsByTrackReference(trackReference);
            TrackDao.DeleteTrack(trackReference);
        }

        public IEnumerable<TrackData> ReadTrackByTitle(string title)
        {
            return ramStorage.Tracks
                .Where(pair => pair.Value.Title == title)
                .Select(pair => pair.Value);
        }
        
        public void InsertSpectralImages(IEnumerable<float[]> spectralImages, string trackId)
        {
            var track = TrackDao.ReadTrackById(trackId);
            if (track == null)
            {
                throw new ArgumentException($"{nameof(trackId)} is not present in the storage");
            }
            
            var images = AssignModelReferences(spectralImages, track);
            SpectralImageDao.InsertSpectralImages(images);
        }

        public IEnumerable<SpectralImageData> GetSpectralImagesByTrackId(string trackId)
        {
            var track = TrackDao.ReadTrackById(trackId);
            if (track == null)
            {
                throw new ArgumentException($"{nameof(trackId)} is not present in the storage");
            }
            
            return SpectralImageDao.GetSpectralImagesByTrackReference(track.TrackReference);
        }
        
        private static IDictionary<string, string> CopyMetaFields(IDictionary<string, string>? metaFields)
        {
            return metaFields == null ? new Dictionary<string, string>() : metaFields.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private static HashedFingerprint ToHashedFingerprint(SubFingerprintData subFingerprint)
        {
            return new HashedFingerprint(subFingerprint.Hashes, subFingerprint.SequenceNumber, subFingerprint.SequenceAt, subFingerprint.OriginalPoint);
        }
        
        private List<SpectralImageData> AssignModelReferences(IEnumerable<float[]> spectralImages, TrackData track)
        {
            int orderNumber = 0;
            return spectralImages.Select(spectralImage => new SpectralImageData(
                    spectralImage,
                    orderNumber++,
                    spectralReferenceProvider.Next(),
                    track.TrackReference))
                .ToList();
        }
    }
}
