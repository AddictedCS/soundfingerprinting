namespace SoundFingerprinting.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Class that holds both audio/video fingerprints.
    /// </summary>
    public class AVRAMStorage : IRAMStorage
    {
        private readonly ILogger<AVRAMStorage> logger;
        private readonly IRAMStorage audio;
        private readonly IRAMStorage video;

        /// <summary>
        /// Initializes a new instance of the <see cref="AVRAMStorage"/> class.
        /// </summary>
        /// <param name="loadFrom">Path to directory that contains persisted audio/video hashes.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public AVRAMStorage(string loadFrom, ILoggerFactory loggerFactory)
        {
            var modelReferenceTracker = new UIntModelReferenceTracker();
            audio = new RAMStorage("audio", modelReferenceTracker, loggerFactory, Path.Combine(loadFrom, "audio"));
            video = new RAMStorage("video", modelReferenceTracker, loggerFactory, Path.Combine(loadFrom, "video"));
            logger = loggerFactory.CreateLogger<AVRAMStorage>();
        }

        /// <inheritdoc cref="IRAMStorage.TracksCount"/>
        public int TracksCount => audio.TracksCount + video.TracksCount;

        /// <inheritdoc cref="IRAMStorage.SubFingerprintsCount"/>
        public int SubFingerprintsCount => audio.SubFingerprintsCount + video.SubFingerprintsCount;

        /// <inheritdoc cref="IRAMStorage.Info"/>
        public IEnumerable<ModelServiceInfo> Info => audio.Info.Concat(video.Info);

        /// <inheritdoc cref="IRAMStorage.GetSubFingerprintsByHashTableAndHash"/>
        public List<uint> GetSubFingerprintsByHashTableAndHash(int table, int hash, MediaType mediaType)
        {
            return mediaType switch
            {
                MediaType.Audio => audio.GetSubFingerprintsByHashTableAndHash(table, hash, MediaType.Audio),
                MediaType.Video => video.GetSubFingerprintsByHashTableAndHash(table, hash, MediaType.Video),
                _ => throw new ArgumentException($"Unknown media type {mediaType}", nameof(mediaType))
            };
        }

        /// <inheritdoc cref="IRAMStorage.Snapshot"/>
        public void Snapshot(string path)
        {
            audio.Snapshot(Path.Combine(path, "audio"));
            video.Snapshot(Path.Combine(path, "video"));
        }

        /// <inheritdoc cref="IRAMStorage.InsertTrack"/>
        public void InsertTrack(TrackInfo track, AVHashes hashes)
        {
            var (audioHashes, videoHashes) = hashes;
            if (track.MediaType.HasFlag(MediaType.Audio) && (audioHashes?.IsEmpty ?? true))
            {
                logger.LogWarning("Track media type is set to Audio, but audio hashes in AVHashes are empty.");
            }
            
            if (track.MediaType.HasFlag(MediaType.Video) && (videoHashes?.IsEmpty ?? true))
            {
                logger.LogWarning("Track media type is set to Video, but video hashes in AVHashes are empty.");
            }
            
            if (!track.MediaType.HasFlag(MediaType.Video) && !(videoHashes?.IsEmpty ?? true))
            {
                throw new ArgumentException("Track media type is set to Audio, but video hashes are non-empty. " +
                                            "If you want to insert both audio and video hashes Track.MediaType has to be set Audio | Video.", nameof(hashes));
            }
            
            if (!track.MediaType.HasFlag(MediaType.Audio) && !(audioHashes?.IsEmpty ?? true))
            {
                throw new ArgumentException("Track media type is set to Video, but audio hashes are non-empty. " +
                                            "If you want to insert both audio and video hashes Track.MediaType has to be set Audio | Video.", nameof(hashes));
            }

            switch (track.MediaType)
            {
                case MediaType.Audio | MediaType.Video:
                    
                    audio.InsertTrack(new TrackInfo(track.Id, track.Title, track.Artist, track.MetaFields, MediaType.Audio), new AVHashes(audioHashes, null, hashes.FingerprintingTime));
                    video.InsertTrack(new TrackInfo(track.Id, track.Title, track.Artist, track.MetaFields, MediaType.Video), new AVHashes(null, videoHashes, hashes.FingerprintingTime));
                    break;
                
                case MediaType.Audio:
                    audio.InsertTrack(track, new AVHashes(audioHashes, null, hashes.FingerprintingTime));
                    break;
                
                case MediaType.Video:
                    video.InsertTrack(track, new AVHashes(null, videoHashes, hashes.FingerprintingTime));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(track.MediaType));
            }
        }

        /// <inheritdoc cref="IRAMStorage.DeleteTrack"/>
        public int DeleteTrack(string id)
        {
            return audio.DeleteTrack(id) + video.DeleteTrack(id);
        }

        /// <inheritdoc cref="IRAMStorage.TryGetTrackByReference"/>
        public bool TryGetTrackByReference(IModelReference trackReference, out TrackData track)
        {
            return audio.TryGetTrackByReference(trackReference, out track) || video.TryGetTrackByReference(trackReference, out track);
        }

        /// <inheritdoc cref="IRAMStorage.TryGetTrackByReference"/>
        public IEnumerable<string> GetTrackIds()
        {
            return audio.GetTrackIds().Concat(video.GetTrackIds()).Distinct();
        }

        /// <inheritdoc cref="IRAMStorage.ReadByTrackId"/>
        public TrackInfo? ReadByTrackId(string id)
        {
            var audioTrack = audio.ReadByTrackId(id);
            var videoTrack = video.ReadByTrackId(id);
            return TracksUtility.CombineTracks(audioTrack, videoTrack);
        }

        /// <inheritdoc cref="IRAMStorage.ReadSubFingerprintsByUid"/>
        public IEnumerable<SubFingerprintData> ReadSubFingerprintsByUid(IEnumerable<uint> ids, MediaType mediaType)
        {
            return mediaType switch
            {
                MediaType.Audio => audio.ReadSubFingerprintsByUid(ids, mediaType),
                MediaType.Video => video.ReadSubFingerprintsByUid(ids, mediaType),
                _ => throw new ArgumentOutOfRangeException(nameof(mediaType))
            };
        }

        /// <inheritdoc cref="IRAMStorage.ReadAvHashesByTrackId"/>
        public AVHashes ReadAvHashesByTrackId(string trackId)
        {
            return new AVHashes(audio.ReadAvHashesByTrackId(trackId).Audio, video.ReadAvHashesByTrackId(trackId).Video);
        }

        /// <inheritdoc cref="IRAMStorage.AddSpectralImages"/>
        public void AddSpectralImages(string trackId, IEnumerable<float[]> images)
        {
            audio.AddSpectralImages(trackId, images);
        }

        /// <inheritdoc cref="IRAMStorage.GetSpectralImagesByTrackReference"/>
        public IEnumerable<SpectralImageData> GetSpectralImagesByTrackReference(string trackId)
        {
            return audio.GetSpectralImagesByTrackReference(trackId);
        }
    }
}