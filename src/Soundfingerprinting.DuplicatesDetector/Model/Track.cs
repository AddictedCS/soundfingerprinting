// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Diagnostics;

namespace Soundfingerprinting.DuplicatesDetector.Model
{
    /// <summary>
    ///   Track entity object
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Id={_id}, Title={_title}, Artist={_artist}, Path={_path}")]
    public class Track
    {
        #region Constants

        /// <summary>
        ///   Maximum artist's length
        /// </summary>
        private const int MAX_ARTIST_LENGTH = 255;

        /// <summary>
        ///   Maximum title's length
        /// </summary>
        private const int MAX_TITLE_LENGTH = 255;

        /// <summary>
        ///   Maximum path's length
        /// </summary>
        private const int MAX_PATH_LENGTH = 255;

        #endregion

        /// <summary>
        ///   Lock object used for concurrency purposes
        /// </summary>
        private static readonly object LockObj = new object();

        /// <summary>
        ///   Incremental Id
        /// </summary>
        private static Int32 _increment;

        #region Private fields

        /// <summary>
        ///   Artist of the track
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _artist;

        /// <summary>
        ///   Id of the track
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private Int32 _id;

        /// <summary>
        ///   Track length
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _path;

        /// <summary>
        ///   Title of the track
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _title;

        /// <summary>
        ///   Track length
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private double _trackLength;

        #endregion

        #region Constructors

        /// <summary>
        ///   Parameter less Constructor
        /// </summary>
        public Track()
        {
            lock (LockObj)
            {
                _id = _increment++;
            }
        }

        /// <summary>
        ///   Track constructor
        /// </summary>
        /// <param name = "artist">Artist's name</param>
        /// <param name = "title">Title</param>
        /// <param name = "path">Path to file to local system</param>
        /// <param name = "length">Length of the file</param>
        public Track(string artist, string title, string path, int length)
            : this()
        {
            Artist = artist;
            Title = title;
            Path = path;
            TrackLength = length;
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Track's id
        /// </summary>
        public Int32 Id
        {
            get { return _id; }
            private set { _id = value; }
        }

        /// <summary>
        ///   Artist's name
        /// </summary>
        public string Artist
        {
            get { return _artist; }
            set { _artist = value.Length > MAX_ARTIST_LENGTH ? value.Substring(0, MAX_ARTIST_LENGTH) : value; }
        }

        /// <summary>
        ///   Track's title
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value.Length > MAX_TITLE_LENGTH ? value.Substring(0, MAX_TITLE_LENGTH) : value; }
        }

        /// <summary>
        ///   Track's Length
        /// </summary>
        public double TrackLength
        {
            get { return _trackLength; }
            set
            {
                if (value < 0)
                    _trackLength = 0;
                _trackLength = value;
            }
        }

        /// <summary>
        ///   Path to file on local system
        /// </summary>
        public string Path
        {
            get { return _path; }
            set { _path = value.Length > MAX_PATH_LENGTH ? value.Substring(0, MAX_PATH_LENGTH) : value; }
        }

        #endregion

        /// <summary>
        ///   Returns hash code of a track object.
        /// </summary>
        /// <returns>Id is returned as it is unique</returns>
        public override int GetHashCode()
        {
            return _id;
        }
    }
}