// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Diagnostics;

namespace Soundfingerprinting.DbStorage.Entities
{
    /// <summary>
    ///   Track entity object
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Title={_title}, Artist={_artist}")]
    public class Track
    {
        #region Private fields

        /// <summary>
        ///   Album id of the track
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private Int32 _albumId;

        /// <summary>
        ///   Artist of the track
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _artist;

        /// <summary>
        ///   Title of the track
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _title;

        /// <summary>
        ///   Track length
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _trackLength;

        #endregion

        #region Constructors

        /// <summary>
        ///   Parameter less Constructor
        /// </summary>
        public Track()
        {
            Id = Int32.MinValue;
        }

        /// <summary>
        ///   Track constructor
        /// </summary>
        /// <param name = "id">Id of the track</param>
        /// <param name = "artist">Artist's name</param>
        /// <param name = "title">Title</param>
        /// <param name = "albumId">Album's identifier</param>
        public Track(int id, string artist, string title, Int32 albumId)
        {
            Id = id;
            Artist = artist;
            Title = title;
            AlbumId = albumId;
        }

        /// <summary>
        ///   Track constructor
        /// </summary>
        /// <param name = "id">Id of the track</param>
        /// <param name = "artist">Artist's name</param>
        /// <param name = "title">Title</param>
        /// <param name = "albumId">Album's identifier</param>
        /// <param name = "trackLength">Track length</param>
        public Track(int id, string artist, string title, Int32 albumId, int trackLength)
            : this(id, artist, title, albumId)
        {
            TrackLength = trackLength;
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Track's id
        /// </summary>
        /// <remarks>
        ///   Once inserted into the database the object will be given a unique identifier
        /// </remarks>
        public Int32 Id { get; set; }

        /// <summary>
        ///   Artist's name
        /// </summary>
        public string Artist
        {
            get { return _artist; }
            set
            {
                if (value.Length > 255)
                    throw new FingerprintEntityException("Artist's length cannot exceed a predefined value. Check the documentation");
                _artist = value;
            }
        }

        /// <summary>
        ///   Track's title
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                if (value.Length > 255)
                    throw new FingerprintEntityException("Title's length cannot exceed a predefined value. Check the documentation");
                _title = value;
            }
        }

        /// <summary>
        ///   Album's Id, in which the track is included
        /// </summary>
        public Int32 AlbumId
        {
            get { return _albumId; }
            set { _albumId = value; }
        }

        /// <summary>
        ///   Track's Length
        /// </summary>
        public int TrackLength
        {
            get { return _trackLength; }
            set
            {
                if (value < 0)
                    throw new FingerprintEntityException("Track's Length cannot be less than 0");
                _trackLength = value;
            }
        }

        #endregion
    }
}