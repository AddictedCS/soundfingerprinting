namespace SoundFingerprinting.DAO.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DAO;

    [Serializable]
    public class SubFingerprintData
    {
        public SubFingerprintData(long[] hashes, int sequenceNumber, double sequenceAt, IModelReference subFingerprintReference, IModelReference trackReference) : this()
        {
            Hashes = hashes;
            SubFingerprintReference = subFingerprintReference;
            TrackReference = trackReference;
            SequenceNumber = sequenceNumber;
            SequenceAt = sequenceAt;
        }

        internal SubFingerprintData()
        {
            Clusters = Enumerable.Empty<string>();
        }

        [IgnoreBinding]
        public long[] Hashes { get; internal set; }

        public int SequenceNumber { get; internal set; }

        public double SequenceAt { get; internal set; }

        [IgnoreBinding]
        public IEnumerable<string> Clusters { get; internal set; }

        [IgnoreBinding]
        public IModelReference SubFingerprintReference { get; internal set; }

        [IgnoreBinding]
        public IModelReference TrackReference { get; internal set; }

        public override bool Equals(object obj)
        {
            if (!(obj is SubFingerprintData))
            {
                return false;
            }

            return ((SubFingerprintData)obj).SubFingerprintReference.Equals(SubFingerprintReference);
        }

        public override int GetHashCode()
        {
            return SubFingerprintReference.GetHashCode();
        }
    }
}
