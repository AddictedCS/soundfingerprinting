namespace SoundFingerprinting.InMemory
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Math;

    /// <summary>
    ///  Class that implements <see cref="IModelService"/> interface.
    /// </summary>
    /// <remarks>
    ///  This implementation is intended to be used for testing purposes only.
    /// </remarks>
    public class InMemoryModelService : IAdvancedModelService
    {
        private readonly IRAMStorage storage;
        private readonly IGroupingCounter groupingCounter;
        private readonly IMetaFieldsFilter metaFieldsFilter = new MetaFieldsFilter();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryModelService"/> class.
        /// </summary>
        /// <param name="loggerFactory">Implementation of <see cref="ILoggerFactory"/> interface.</param>
        public InMemoryModelService(ILoggerFactory? loggerFactory = null) : this(
            new AVRAMStorage(string.Empty, loggerFactory ?? new NullLoggerFactory()), new StandardGroupingCounter())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryModelService"/> class.
        /// </summary>
        /// <param name="loadFrom">Path to directory from which to load previously stored instance of <see cref="InMemoryModelService"/>.</param>
        /// <param name="loggerFactory">Implementation of <see cref="ILoggerFactory"/> interface.</param>
        /// <remarks>
        ///  Previous to v8, path parameter was path to a file. Since v8 it has to be path to a directory.
        ///  If you are migrating from previous version and want to migrate the snapshot as well, rename previous snapshots to "audio", and place it in the provided directory parameter.
        /// </remarks>
        public InMemoryModelService(string loadFrom, ILoggerFactory? loggerFactory = null) : this(
            new AVRAMStorage(loadFrom, loggerFactory ?? new NullLoggerFactory()), new StandardGroupingCounter())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryModelService"/> class.
        /// </summary>
        /// <param name="groupingCounter">Implementation of <see cref="IGroupingCounter"/> interface.</param>
        /// <param name="loggerFactory">Implementation of <see cref="ILoggerFactory"/> interface.</param>
        public InMemoryModelService(IGroupingCounter groupingCounter, ILoggerFactory? loggerFactory = null) : this(
            new AVRAMStorage(string.Empty, loggerFactory ?? new NullLoggerFactory()), groupingCounter)
        {
        }

        private InMemoryModelService(IRAMStorage storage, IGroupingCounter groupingCounter)
        {
            this.storage = storage;
            this.groupingCounter = groupingCounter;
        }

        /// <summary>
        ///  Path to directory that will store files associated with this instance of <see cref="InMemoryModelService"/>.
        /// </summary>
        /// <param name="path">Path to directory.</param>
        public void Snapshot(string path)
        {
            storage.Snapshot(path);
        }

        /// <inheritdoc cref="IModelService.Info"/>
        public IEnumerable<ModelServiceInfo> Info => storage.Info;

        /// <inheritdoc cref="IModelService.Insert"/>
        public void Insert(TrackInfo track, AVHashes avHashes)
        {
            storage.InsertTrack(track, avHashes);
        }

        /// <inheritdoc cref="IModelService.UpdateTrack"/>
        public void UpdateTrack(TrackInfo trackInfo)
        {
            var track = GetTrackById(trackInfo.Id);
            if (track == null)
            {
                throw new ArgumentException($"Could not find track {trackInfo.Id} to update", nameof(trackInfo.Id));
            }

            if (trackInfo.MediaType != track.MediaType)
            {
                throw new ArgumentException($"Can't update media type from {trackInfo.MediaType} to {track.MediaType}. Delete {track.Id} and reinsert with new media type.");
            }

            var hashes = storage.ReadAvHashesByTrackId(trackInfo.Id);
            DeleteTrack(trackInfo.Id);
            Insert(trackInfo, hashes);
        }

        /// <inheritdoc cref="IModelService.Query"/>
        public AVSubFingerprints Query(AVHashes hashes, AVQueryConfiguration config)
        {
            var (audioHashes, videoHashes) = hashes;
            var audioSubFingerprints = QueryHashesWithMediaType(audioHashes, config.Audio, MediaType.Audio);
            var videoSubFingerprints = QueryHashesWithMediaType(videoHashes, config.Video, MediaType.Video);
            return new AVSubFingerprints(audioSubFingerprints, videoSubFingerprints);
        }

        private IEnumerable<SubFingerprintData> QueryHashesWithMediaType(Hashes? hashes, QueryConfiguration config, MediaType mediaType)
        {
            var queryHashes = hashes?.Select(_ => _.HashBins).ToList() ?? Enumerable.Empty<int[]>().ToList();
            return queryHashes.Any() ? ReadSubFingerprints(queryHashes, mediaType, config) : Enumerable.Empty<SubFingerprintData>();
        }

        /// <inheritdoc cref="IModelService.ReadHashesByTrackId"/>
        public AVHashes ReadHashesByTrackId(string trackId)
        {
            var track = GetTrackById(trackId);
            if (track == null)
            {
                return AVHashes.Empty;
            }

            return storage.ReadAvHashesByTrackId(trackId);
        }

        /// <inheritdoc cref="IModelService.GetTrackIds"/>
        public IEnumerable<string> GetTrackIds()
        {
            return storage.GetTrackIds();
        }

        /// <inheritdoc cref="IModelService.ReadTracksByReferences"/>
        public IEnumerable<TrackData> ReadTracksByReferences(IEnumerable<IModelReference> references)
        {
            return references.Aggregate(new List<TrackData>(), (list, reference) =>
            {
                if (storage.TryGetTrackByReference(reference, out var track))
                {
                    list.Add(track);
                }

                return list;
            });
        }

        /// <inheritdoc cref="IModelService.ReadTrackById"/>
        public TrackInfo? ReadTrackById(string trackId)
        {
            var trackData = GetTrackById(trackId);
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
            var track = GetTrackById(trackId);
            if (track == null)
            {
                return;
            }

            var trackReference = track.TrackReference;
            storage.DeleteTrack(trackReference);
        }

        /// <inheritdoc cref="IAdvancedModelService.InsertSpectralImages"/>
        public void InsertSpectralImages(IEnumerable<float[]> spectralImages, string trackId)
        {
            var track = GetTrackById(trackId);
            if (track == null)
            {
                throw new ArgumentException($"{nameof(trackId)} is not present in the storage");
            }
            
            storage.AddSpectralImages(track.TrackReference, spectralImages);
        }

        /// <inheritdoc cref="IAdvancedModelService.GetSpectralImagesByTrackId"/>
        public IEnumerable<SpectralImageData> GetSpectralImagesByTrackId(string trackId)
        {
            var track = GetTrackById(trackId);
            if (track == null)
            {
                throw new ArgumentException($"{nameof(trackId)} is not present in the storage");
            }
            
            return storage.GetSpectralImagesByTrackReference(track.TrackReference);
        }
        
        private static IDictionary<string, string> CopyMetaFields(IDictionary<string, string>? metaFields)
        {
            return metaFields == null ? new Dictionary<string, string>() : metaFields.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private TrackData? GetTrackById(string trackId)
        {
            return storage.ReadByTrackId(trackId);
        }
        
        private IEnumerable<SubFingerprintData> ReadSubFingerprints(IEnumerable<int[]> hashes, MediaType mediaType, QueryConfiguration queryConfiguration)
        {
            var allSubs = new ConcurrentDictionary<uint, byte>();
            int threshold = queryConfiguration.ThresholdVotes;

            hashes.AsParallel().ForAll(hashedFingerprint => 
            {
                var ids = QuerySubFingerprints(hashedFingerprint, threshold, mediaType);
                foreach (uint subFingerprint in ids)
                {
                    allSubs.TryAdd(subFingerprint, 0);
                }
            });

            return ResolveFromIds(allSubs.Keys, queryConfiguration.YesMetaFieldsFilters, queryConfiguration.NoMetaFieldsFilters, mediaType);
        }
        
        private IEnumerable<uint> QuerySubFingerprints(int[] hashes, int thresholdVotes, MediaType mediaType)
        {
            var results = new List<uint>[hashes.Length];
            for (int table = 0; table < hashes.Length; ++table)
            {
                int hashBin = hashes[table];
                results[table] = storage.GetSubFingerprintsByHashTableAndHash(table, hashBin, mediaType);
            }

            return groupingCounter.GroupByAndCount(results, thresholdVotes);
        }

        private IEnumerable<SubFingerprintData> ResolveFromIds(IEnumerable<uint> ids,
            IDictionary<string, string> yesMetaFieldsFilters,
            IDictionary<string, string> noMetaFieldsFilters,
            MediaType mediaType)
        {
            return storage.ReadSubFingerprintsByUid(ids, mediaType)
                .GroupBy(subFingerprint => subFingerprint.TrackReference)
                .Where(group =>
                {
                    if (storage.TryGetTrackByReference(group.Key, out var trackData))
                    {
                        return metaFieldsFilter.PassesFilters(trackData.MetaFields, yesMetaFieldsFilters, noMetaFieldsFilters);
                    }

                    return false;
                })
                .SelectMany(x => x.ToList());
        }
    }
}
