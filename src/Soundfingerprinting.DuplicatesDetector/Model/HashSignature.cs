// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Diagnostics;

namespace Soundfingerprinting.DuplicatesDetector.Model
{
    /// <summary>
    ///   Hash signature
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Id={_id}")]
    public class HashSignature
    {
        /// <summary>
        ///   Incremental Id
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static Int32 _increment;

        /// <summary>
        ///   Lock object
        /// </summary>
        private static readonly object LockObject = new object();

        /// <summary>
        ///   Id
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly Int32 _id;

        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name = "track">Hashed track</param>
        /// <param name = "signature">Signature of the track</param>
        public HashSignature(Track track, int[] signature)
        {
            Track = track;
            Signature = signature;
            lock (LockObject)
            {
                _id = _increment++;
            }
        }

        /// <summary>
        ///   Signature of the track
        /// </summary>
        public int[] Signature { get; private set; }

        /// <summary>
        ///   Track (hashed)
        /// </summary>
        public Track Track { get; private set; }

        /// <summary>
        ///   Id of the hash
        /// </summary>
        public Int32 Id
        {
            get { return _id; }
        }

        public override int GetHashCode()
        {
            return _id;
        }
    }
}