﻿namespace SoundFingerprinting.InMemory
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.Extensions.Logging;
    using ProtoBuf;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    [Serializable]
    [ProtoContract(SkipConstructor = true)]
    public class RAMStorage : IRAMStorage
    {
        private readonly IModelReferenceTracker<uint> modelReferenceTracker;
        private readonly ILogger<RAMStorage> logger;
        private readonly ConcurrentDictionary<int, List<uint>>[] hashTables;

        private readonly string id;

        [ProtoMember(3)] 
        private int numberOfHashTables;

        [ProtoMember(4)] 
        private IDictionary<IModelReference, TrackData> tracks;
        
        [ProtoMember(5)]
        private ConcurrentDictionary<uint, SubFingerprintData> subFingerprints;

        /// <summary>
        /// Initializes a new instance of the <see cref="RAMStorage"/> class.
        /// </summary>
        /// <param name="id">ID of the RAM storage.</param>
        /// <param name="modelReferenceTracker">Model reference tracker.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        /// <param name="loadFrom">Initialize from file.</param>
        /// <param name="numberOfHashTables">Number of hashes tables.</param>
        public RAMStorage(string id, IModelReferenceTracker<uint> modelReferenceTracker, ILoggerFactory loggerFactory, string loadFrom = "", int numberOfHashTables = 25)
        {
            logger = loggerFactory.CreateLogger<RAMStorage>();
            this.id = id;
            this.numberOfHashTables = numberOfHashTables;
            this.modelReferenceTracker = modelReferenceTracker;
            tracks = new ConcurrentDictionary<IModelReference, TrackData>();
            subFingerprints = new ConcurrentDictionary<uint, SubFingerprintData>();
            
            logger.LogDebug("Initializing {Count} hash tables", numberOfHashTables);
            hashTables = new ConcurrentDictionary<int, List<uint>>[numberOfHashTables];
            for (int table = 0; table < numberOfHashTables; table++)
            {
                hashTables[table] = new ConcurrentDictionary<int, List<uint>>();
            } 
            
            InitializeFromFile(loadFrom);

            IModelReference? lastTrackReference = null;
            uint maxTrackId = 0;
            if (tracks.Any())
            {
                (lastTrackReference, maxTrackId) = tracks.Keys
                    .Select(_ => (_, _.Get<uint>()))
                    .OrderByDescending(_ => _.Item2)
                    .First();
            }

            uint maxSubFingerprintId = 0;
            if (lastTrackReference != null)
            {
                maxSubFingerprintId = ReadSubFingerprintByTrackReference(lastTrackReference).Max(_ => _.SubFingerprintReference.Get<uint>());
            }

            logger.LogDebug("Resetting track ref to {MaxTrackId}, fingerprints ref to {MaxSubFingerprintId}", maxTrackId, maxSubFingerprintId);
            modelReferenceTracker.TryResetTrackRef(maxTrackId);
            modelReferenceTracker.TryResetSubFingerprintRef(maxSubFingerprintId);
        }
        
        /// <inheritdoc cref="IRAMStorage.TracksCount"/>
        public int TracksCount => tracks.Count;

        /// <inheritdoc cref="IRAMStorage.SubFingerprintsCount"/>
        public int SubFingerprintsCount => subFingerprints.Count;

        /// <inheritdoc cref="IRAMStorage.Info"/>
        public IEnumerable<ModelServiceInfo> Info
        {
            get
            {
                return new[] { new ModelServiceInfo(id, TracksCount, SubFingerprintsCount, hashTables.Select(table => table.Count).ToArray()) };
            }
        }

        /// <inheritdoc cref="IRAMStorage.InsertTrack"/>
        public void InsertTrack(TrackInfo track, AVHashes avHashes)
        {
            if (track.MediaType.HasFlag(MediaType.Audio | MediaType.Video))
            {
                throw new ArgumentException("RAM storage is designed to handle only one media type of tracks. To keep both audio and video user AVRAMStorage", nameof(track));
            }
            
            var (audio, video) = avHashes;
            if (!(audio?.IsEmpty ?? true) && !(video?.IsEmpty ?? true))
            {
                throw new ArgumentException("RAM storage is designed to handle only one media type of for hashes. To keep both audio and video use AVRAMStorage", nameof(avHashes));
            }

            var hashes = audio ?? video;
            var (trackData, fingerprints) = modelReferenceTracker.AssignModelReferences(track, hashes!);
            tracks[trackData.TrackReference] = trackData;
            foreach (var subFingerprint in fingerprints)
            {
                AddSubFingerprint(subFingerprint);
            }
        }

        /// <summary>
        ///  Adds one sub-fingerprint to RAM storage.
        /// </summary>
        /// <param name="subFingerprintData">Sub-fingerprint to add.</param>
        public void AddSubFingerprint(SubFingerprintData subFingerprintData)
        {
            subFingerprints[subFingerprintData.SubFingerprintReference.Get<uint>()] = subFingerprintData;
            InsertHashes(subFingerprintData.Hashes, subFingerprintData.SubFingerprintReference.Get<uint>());
        }

        /// <inheritdoc cref="IRAMStorage.InsertTrack"/>
        public AVHashes ReadAvHashesByTrackId(string trackId)
        {
            var track = ReadTrackDataByTrackId(trackId);
            if (track == null)
            {
                return AVHashes.Empty;
            }

            var fingerprints = ReadSubFingerprintByTrackReference(track.TrackReference).Select(ToHashedFingerprint);
            return track.MediaType switch
            {
                MediaType.Audio => new AVHashes(new Hashes(fingerprints, track.Length, track.MediaType), null),
                MediaType.Video => new AVHashes(null, new Hashes(fingerprints, track.Length, track.MediaType)),
                _ => throw new InvalidOperationException($"Unknown track media type {track.MediaType}")
            };
        }

        private static HashedFingerprint ToHashedFingerprint(SubFingerprintData subFingerprint)
        {
            return new HashedFingerprint(subFingerprint.Hashes, subFingerprint.SequenceNumber, subFingerprint.SequenceAt, subFingerprint.OriginalPoint);
        }

        /// <inheritdoc cref="IRAMStorage.DeleteTrack"/>
        public int DeleteTrack(string id)
        {
            var data = ReadTrackDataByTrackId(id);
            if (data == null)
            {
                return 0;
            }

            var trackReference = data.TrackReference;
            int modified = DeleteSubFingerprintsByTrackReference(trackReference);
            if (tracks.Remove(trackReference))
            {
                return modified + 1;
            }

            return modified;
        }

        private int DeleteSubFingerprintsByTrackReference(IModelReference trackReference)
        {
            var all = ReadSubFingerprintByTrackReference(trackReference).ToList();
            foreach (var reference in all)
            {
                subFingerprints.TryRemove(reference.SubFingerprintReference.Get<uint>(), out _);
            }

            int totals = 0;
            all.AsParallel().ForAll(subFingerprintData =>
            {
                int[] hashes = subFingerprintData.Hashes;
                for (int table = 0; table < hashes.Length; ++table)
                {
                    if (hashTables[table].TryGetValue(hashes[table], out var list))
                    {
                        lock (list)
                        {
                            int removed = list.RemoveAll(id => id == subFingerprintData.SubFingerprintReference.Get<uint>());
                            Interlocked.Add(ref totals, removed);
                        }
                    }
                }
            });

            return totals + all.Count;
        }

        /// <inheritdoc cref="IRAMStorage.TryGetTrackByReference"/>
        public bool TryGetTrackByReference(IModelReference trackReference, out TrackData track)
        {
            return tracks.TryGetValue(trackReference, out track);
        }

        /// <inheritdoc cref="IRAMStorage.GetTrackIds"/>
        public IEnumerable<string> GetTrackIds()
        {
            return tracks.Values.Select(_ => _.Id);
        }

        /// <inheritdoc cref="IRAMStorage.ReadByTrackId"/>
        public TrackInfo? ReadByTrackId(string id)
        {
            var data = ReadTrackDataByTrackId(id);
            if (data == null)
            {
                return null;
            }
            
            return new TrackInfo(data.Id, data.Title, data.Artist, data.MetaFields, data.MediaType);
        }

        /// <inheritdoc cref="IRAMStorage.ReadSubFingerprintsByUid"/>
        public IEnumerable<SubFingerprintData> ReadSubFingerprintsByUid(IEnumerable<uint> ids, MediaType mediaType)
        {
            return ids.AsParallel()
                .Select(id => subFingerprints.TryGetValue(id, out var s) ? s : null)
                .Where(s => s != null)
                .Select(s => s!)
                .ToList();
        }

        private IEnumerable<SubFingerprintData> ReadSubFingerprintByTrackReference(IModelReference trackReference)
        {
            return subFingerprints.Where(pair => pair.Value.TrackReference.Equals(trackReference)).Select(pair => pair.Value).ToList();
        }

        private void InitializeFromFile(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }
            
            using var file = File.OpenRead(path);
            var obj = Serializer.Deserialize<RAMStorage>(file);
            numberOfHashTables = obj.numberOfHashTables;
            tracks = obj.tracks ?? new ConcurrentDictionary<IModelReference, TrackData>();
            subFingerprints = obj.subFingerprints ?? new ConcurrentDictionary<uint, SubFingerprintData>();
            foreach (KeyValuePair<uint, SubFingerprintData> pair in subFingerprints)
            {
                InsertHashes(pair.Value.Hashes, pair.Key);
            }
            
            logger.LogInformation("Reloaded storage from {Path}, tracks=[{TracksCount}], fingerprints=[{SubFingerprintsCount}]", path, TracksCount, SubFingerprintsCount);
        }

        /// <inheritdoc cref="IRAMStorage.GetSubFingerprintsByHashTableAndHash"/>
        public List<uint> GetSubFingerprintsByHashTableAndHash(int table, int hash, MediaType mediaType)
        {
            return GetSubFingerprintsByHashTableAndHash(table, hash);
        }

        private List<uint> GetSubFingerprintsByHashTableAndHash(int table, int hash)
        {
            if (hashTables[table].TryGetValue(hash, out var subFingerprintIds))
            {
                return subFingerprintIds;
            }

            return Enumerable.Empty<uint>().ToList();
        }

        /// <inheritdoc cref="IRAMStorage.Snapshot"/>
        public void Snapshot(string path)
        {
            using var file = File.Create(path);
            Serializer.Serialize(file, this);
        }

        private void InsertHashes(int[] hashBins, uint subFingerprintId)
        {
            int table = 0;
            foreach (var hashBin in hashBins)
            {
                var hashTable = hashTables[table];
                var ids = hashTable.GetOrAdd(hashBin, _ => new List<uint>());
                lock (ids)
                {
                    ids.Add(subFingerprintId);
                }

                table++;
            }
        }

        private TrackData? ReadTrackDataByTrackId(string id)
        {
            return tracks.Values.FirstOrDefault(pair => pair.Id == id);
        }
    }
}