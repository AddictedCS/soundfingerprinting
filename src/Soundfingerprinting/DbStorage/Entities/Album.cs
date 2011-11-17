// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Diagnostics;

namespace Soundfingerprinting.DbStorage.Entities
{
    /// <summary>
    ///   Album entity class
    /// </summary>
    /// <remarks>
    ///   This class represents an album instance. Default values for member fields:
    ///   <c>Id = -1</c>
    ///   <c>ReleaseYear = 1900</c>
    ///   <c>Name = "Unknown"</c>
    /// </remarks>
    [Serializable]
    [DebuggerDisplay("Name={_name}, Year={_releaseYear}")]
    public class Album
    {
        #region Private Fields

        /// <summary>
        ///   Name of the album
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _name;

        /// <summary>
        ///   Albums release year
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _releaseYear;

        #endregion

        #region Constructors

        /// <summary>
        ///   Parameter less constructor
        /// </summary>
        public Album()
        {
            Id = Int32.MinValue;
            _releaseYear = 1500; /*Check docs*/
            _name = "Unknown";
        }

        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name = "id">Id of the album</param>
        /// <param name = "name">Name of the album</param>
        public Album(int id, string name)
        {
            _releaseYear = 1500; /*Check docs*/
            Id = id;
            _name = name;
        }

        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name = "id">Id of the album</param>
        /// <param name = "name">Name of the album</param>
        /// <param name = "releaseYear">Album's release year [1900..2100]</param>
        public Album(int id, string name, int releaseYear)
            : this(id, name)
        {
            _releaseYear = releaseYear;
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Album's name
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (value.Length > 255 /*DB Size Item*/)
                    throw new FingerprintEntityException("Name length cannot exceed a predefined value. Check the documentation");
                _name = value;
            }
        }

        /// <summary>
        ///   Album's Id
        /// </summary>
        /// <remarks>
        ///   Once inserted into the database the object will be given a unique identifier
        /// </remarks>
        public Int32 Id { get; set; }

        /// <summary>
        ///   Album's release date
        /// </summary>
        public int ReleaseYear
        {
            get { return _releaseYear; }
            set
            {
                if (value > 1500 && value < 2100)
                    _releaseYear = value;
                else
                    throw new FingerprintEntityException("ReleaseYear does not match a predefined range. Check the documentation");
            }
        }

        #endregion
    }
}