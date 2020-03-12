// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace SoundFingerprinting.Data
{
    using System;
    using ProtoBuf;
    using SoundFingerprinting.LCS;

    [ProtoContract]
    public class QueryMatch
    {
        public QueryMatch(string queryMatchId, TrackInfo track, Coverage coverage, DateTime matchedAt)
        {
            QueryMatchId = queryMatchId;
            Track = track;
            Coverage = coverage;
            MatchedAt = matchedAt;
        }
        
        private QueryMatch()
        {
            // left for proto-buf
        }
        
        [ProtoMember(1)]
        public TrackInfo Track { get; }
        
        [ProtoMember(2)]
        public Coverage Coverage { get; }

        [ProtoMember(3)]
        public DateTime MatchedAt { get; }

        [ProtoMember(4)]
        public string QueryMatchId { get; }
    }
}