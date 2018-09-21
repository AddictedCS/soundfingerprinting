namespace SoundFingerprinting.Data
{
    using System;
    using System.Collections.Generic;

    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class FingerprintsQueryResponse
    {
        public FingerprintsQueryResponse(IEnumerable<SubFingerprintInfo> subFingerprints)
        {
            SubFingerprints = subFingerprints;
        }

        [ProtoMember(1)]
        public IEnumerable<SubFingerprintInfo> SubFingerprints { get; }
    }
}
