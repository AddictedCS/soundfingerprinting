namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.Data;

    public class AVQueryResult
    {
        public AVQueryResult(IEnumerable<AVResultEntry> resultEntries, AVHashes queryHashes, AVQueryCommandStats queryCommandStats)
        {
            ResultEntries = resultEntries;
            QueryHashes = queryHashes;
            QueryCommandStats = queryCommandStats;
        }

        public AVHashes QueryHashes { get; }
        
        public AVQueryCommandStats QueryCommandStats { get; }

        public AVResultEntry BestMatch => ResultEntries.FirstOrDefault();

        public bool ContainsMatches => ResultEntries != null && ResultEntries.Any();

        public IEnumerable<AVResultEntry> ResultEntries { get; }

        public string StreamId => QueryHashes.Audio?.StreamId ?? QueryHashes.Video?.StreamId ?? string.Empty;

        public static IEnumerable<AVResultEntry> JoinResultEntries(QueryResult? audio, QueryResult? video)
        {
            var empty = Enumerable.Empty<ResultEntry>();
            var audioEntries = audio?.ResultEntries ?? empty;
            var videoEntries = video?.ResultEntries ?? empty;

            Func<ResultEntry, string> onTrackId = e => e.Track.Id;
            var audioEntriesList = audioEntries.ToList();
            var videoEntriesList = videoEntries.ToList();
            var pairs = audioEntriesList.Join(videoEntriesList, onTrackId, onTrackId,
                (a, v) => new AVResultEntry(a, v)).ToList();
            var pairIds = new HashSet<string>(pairs.Select(j => j.Track.Id));
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

        public AVQueryResult WithFingerprintingDurationMilliseconds(long audioFingerprinting, long videoFingerprinting)
        {
            return new AVQueryResult(ResultEntries, QueryHashes, QueryCommandStats.WithFingerprintingDurationMilliseconds(audioFingerprinting, videoFingerprinting));
        }

        public static AVQueryResult Empty(AVHashes hashes)
        {
            return new AVQueryResult(Enumerable.Empty<AVResultEntry>(), hashes, new AVQueryCommandStats(Query.QueryCommandStats.Zero(), Query.QueryCommandStats.Zero()));
        }
    }
}