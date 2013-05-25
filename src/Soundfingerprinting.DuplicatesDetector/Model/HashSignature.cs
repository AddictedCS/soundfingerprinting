namespace Soundfingerprinting.DuplicatesDetector.Model
{
    using System;
    using System.Threading;

    /// <summary>
    ///   Hash signature
    /// </summary>
    [Serializable]
    public class HashSignature
    {
        private static int increment;
        
        private readonly int id;

        public HashSignature(Track track, long[] signature)
        {
            Track = track;
            Signature = signature;
            id = Interlocked.Increment(ref increment);
        }

        public int Id
        {
            get { return id; }
        }

        public long[] Signature { get; private set; }

        public Track Track { get; private set; }
        
        public override int GetHashCode()
        {
            return id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return id == ((HashSignature)obj).Id;
        }
    }
}