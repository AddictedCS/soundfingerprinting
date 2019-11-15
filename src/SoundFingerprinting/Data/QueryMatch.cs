// ReSharper disable MemberCanBePrivate.Global
namespace SoundFingerprinting.Data
{
    using System;
    using System.Collections.Generic;
    using ProtoBuf;

    [ProtoContract]
    public class QueryMatch
    {
        public QueryMatch(string queryMatchId, string trackId, double queryMatchLength, double queryMatchStartsAt, double trackStartsAt, double trackMatchStartsAt, double confidence, double queryLength, DateTime matchedAt, double trackLength)
        : this(queryMatchId, trackId, queryMatchLength, queryMatchStartsAt, trackStartsAt, trackMatchStartsAt, confidence, queryLength, matchedAt, trackLength, new Dictionary<string, string>())
        {
        }
        
        public QueryMatch(string queryMatchId, string trackId, double queryMatchLength, double queryMatchStartsAt, double trackStartsAt, double trackMatchStartsAt, double confidence, double queryLength, DateTime matchedAt, double trackLength, Dictionary<string, string> meta)
        {
            QueryMatchId = queryMatchId;
            TrackId = trackId;
            QueryMatchLength = queryMatchLength;
            QueryMatchStartsAt = queryMatchStartsAt;
            TrackStartsAt = trackStartsAt;
            TrackMatchStartsAt = trackMatchStartsAt;
            Confidence = confidence;
            QueryLength = queryLength;
            MatchedAt = matchedAt;
            TrackLength = trackLength;
            Meta = meta;
        }

        private QueryMatch()
        {
            // left for proto-buf
        }
        
        [ProtoMember(1)]
        public string TrackId { get; }
        
        [ProtoMember(2)]
        public double QueryMatchLength { get; }

        [ProtoMember(3)]
        public double QueryMatchStartsAt { get; }

        [ProtoMember(4)]
        public double TrackStartsAt { get; }

        [ProtoMember(5)]
        public double TrackMatchStartsAt { get; }
        
        [ProtoMember(6)]
        public double Confidence { get; }

        [ProtoMember(7)]
        public double QueryLength { get; }

        [ProtoMember(8)]
        public DateTime MatchedAt { get; }

        [ProtoMember(9)]
        public double TrackLength { get; }
        
        [ProtoMember(10)]
        public Dictionary<string, string> Meta { get; }

        [ProtoMember(11)]
        public string QueryMatchId { get; }

        public double Coverage => QueryMatchLength / TrackLength;

    }
}