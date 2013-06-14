namespace SoundFingerprinting.DuplicatesDetector.Model
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    ///   Music track
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Id={id}, Title={title}, Artist={artist}, Path={path}")]
    public class Track
    {
        #region Constants

        /// <summary>
        ///   Maximum artist's length
        /// </summary>
        private const int MaxArtistLength = 255;

        /// <summary>
        ///   Maximum title's length
        /// </summary>
        private const int MaxTitleLength = 255;

        /// <summary>
        ///   Maximum path's length
        /// </summary>
        private const int MaxPathLength = 255;

        #endregion

        /// <summary>
        ///   Incremental Id
        /// </summary>
        private static int increment;

        #region Private fields

        /// <summary>
        ///   Id of the track
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly int id;

        /// <summary>
        ///   Artist of the track
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
        private string artist;

        /// <summary>
        ///   Track length
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
        private string path;

        /// <summary>
        ///   Title of the track
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string title;

        /// <summary>
        ///   Track length
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
        private double trackLength;

        #endregion

        public Track(string artist, string title, string path, double length)
            : this()
        {
            Artist = artist;
            Title = title;
            Path = path;
            TrackLength = length;
        }

        protected Track()
        {
            id = Interlocked.Increment(ref increment);
        }

        #region Properties

        public int Id
        {
            get { return id; }
        }

        public string Artist
        {
            get { return artist; }
            set { artist = value.Length > MaxArtistLength ? value.Substring(0, MaxArtistLength) : value; }
        }

        public string Title
        {
            get { return title; }
            set { title = value.Length > MaxTitleLength ? value.Substring(0, MaxTitleLength) : value; }
        }

        public double TrackLength
        {
            get
            {
                return trackLength;
            }

            set
            {
                if (value < 0)
                {
                    trackLength = 0;
                }

                trackLength = value;
            }
        }

        public string Path
        {
            get { return path; }
            set { path = value.Length > MaxPathLength ? value.Substring(0, MaxPathLength) : value; }
        }

        #endregion

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

            return id == ((Track)obj).Id;
        }
    }
}