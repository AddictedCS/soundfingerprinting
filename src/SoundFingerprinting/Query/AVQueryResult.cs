namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Class that contains all the information related to audio/video matches.
    /// </summary>
    public class AVQueryResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AVQueryResult"/> class.
        /// </summary>
        /// <param name="audio">An instance of <see cref="QueryResult"/> class representing audio query result.</param>
        /// <param name="video">An instance of <see cref="QueryResult"/> class representing video query result.</param>
        /// <param name="queryHashes">Query hashes used for querying.</param>
        /// <param name="queryCommandStats">Query command statistics.</param>
        public AVQueryResult(QueryResult? audio, QueryResult? video, AVHashes queryHashes, AVQueryCommandStats queryCommandStats) : this(audio, video, JoinResultEntries(audio, video), queryHashes, queryCommandStats)
        {
            // no op
        }

        internal AVQueryResult(QueryResult? audio, QueryResult? video, IEnumerable<AVResultEntry> resultEntries, AVHashes queryHashes, AVQueryCommandStats queryCommandStats)
        {
            Audio = audio;
            Video = video;
            ResultEntries = resultEntries;
            QueryHashes = queryHashes;
            QueryCommandStats = queryCommandStats; 
        }

        /// <summary>
        ///  Gets audio query result.
        /// </summary>
        public QueryResult? Audio { get; }

        /// <summary>
        ///  Gets video query result.
        /// </summary>
        public QueryResult? Video { get; }
        
        /// <summary>
        ///  Gets query hashes used for for querying the model service <see cref="IModelService.Query"/>.
        /// </summary>
        public AVHashes QueryHashes { get; }
        
        /// <summary>
        ///  Gets query command statistics.
        /// </summary>
        public AVQueryCommandStats QueryCommandStats { get; }

        /// <summary>
        ///  Gets query best match.
        /// </summary>
        public AVResultEntry? BestMatch => ResultEntries.FirstOrDefault();

        /// <summary>
        ///  Gets a value indicating whether query result contains matches.
        /// </summary>
        public bool ContainsMatches => ResultEntries.Any();

        /// <summary>
        ///  Gets the list of audio/video result entries.
        /// </summary>
        public IEnumerable<AVResultEntry> ResultEntries { get; }

        /// <summary>
        ///  Gets stream id associated with the query result.
        /// </summary>
        public string StreamId => QueryHashes.Audio?.StreamId ?? QueryHashes.Video?.StreamId ?? string.Empty;
        
        /// <summary>
        ///  Creates a new instance of <see cref="AVQueryResult"/> class with updated fingerprinting times.
        /// </summary>
        /// <param name="audioFingerprinting">Audio fingerprinting time.</param>
        /// <param name="videoFingerprinting">Video fingerprinting time.</param>
        /// <returns>An instance of <see cref="AVQueryResult"/> class.</returns>
        public AVQueryResult WithFingerprintingDurationMilliseconds(long audioFingerprinting, long videoFingerprinting)
        {
            return new AVQueryResult(Audio, Video, ResultEntries, QueryHashes, QueryCommandStats.WithFingerprintingDurationMilliseconds(audioFingerprinting, videoFingerprinting));
        }

        /// <summary>
        ///  Deconstructs audio/video query result object.
        /// </summary>
        /// <param name="audio">Audio query result.</param>
        /// <param name="video">Video query result.</param>
        public void Deconstruct(out QueryResult? audio, out QueryResult? video)
        {
            audio = Audio;
            video = Video;
        }

        private static IEnumerable<AVResultEntry> JoinResultEntries(QueryResult? audio, QueryResult? video)
        {
            var empty = Enumerable.Empty<ResultEntry>();
            var audioEntries = audio?.ResultEntries ?? empty;
            var videoEntries = video?.ResultEntries ?? empty;

            Func<ResultEntry, string> onTrackId = e => e.Track.Id;
            var audioEntriesList = audioEntries.ToList();
            var videoEntriesList = videoEntries.ToList();
            var pairs = audioEntriesList.Join(videoEntriesList, onTrackId, onTrackId,
                (a, v) => new AVResultEntry(a, v)).ToList();
            var pairIds = new HashSet<string>(pairs.Select(j => j.TrackId));
            var audioOnly = audioEntriesList
                .Where(a => !pairIds.Contains(a.Track.Id))
                .Select(a => new AVResultEntry(a, null))
                .ToList();
            var videoOnly = videoEntriesList
                .Where(v => !pairIds.Contains(v.Track.Id))
                .Select(v => new AVResultEntry(null, v))
                .ToList();

            return pairs.Concat(audioOnly).Concat(videoOnly).ToList();
        }

        /// <summary>
        ///  Creates an empty audio/video query result object.
        /// </summary>
        /// <param name="hashes">Hashes used for querying.</param>
        /// <returns>An empty instance of <see cref="AVQueryResult"/> class.</returns>
        public static AVQueryResult Empty(AVHashes hashes)
        {
            var (audioHashes, videoHashes) = hashes;
            return new AVQueryResult(audioHashes != null ? QueryResult.Empty(audioHashes, 0) : null, videoHashes != null ? QueryResult.Empty(videoHashes, 0) : null, Enumerable.Empty<AVResultEntry>(), hashes, new AVQueryCommandStats(Query.QueryCommandStats.Zero(), Query.QueryCommandStats.Zero()));
        }
    }
}