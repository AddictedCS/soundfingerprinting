namespace SoundFingerprinting.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class FingerprintsQueryResponse
    {
        public FingerprintsQueryResponse(IEnumerable<QueryResponseMatch> matches)
        {
            Matches = matches;
        }

        private FingerprintsQueryResponse()
        {
            // left for protobuf
        }

        public static FingerprintsQueryResponse Empty { get; } = new FingerprintsQueryResponse(Enumerable.Empty<QueryResponseMatch>());

        [ProtoMember(1)]
        public IEnumerable<QueryResponseMatch> Matches { get; }

        public bool IsEmpty => Matches == null || !Matches.Any();
    }
}
