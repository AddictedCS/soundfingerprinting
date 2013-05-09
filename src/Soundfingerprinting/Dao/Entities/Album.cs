namespace Soundfingerprinting.Dao.Entities
{
    using System;
    using System.Diagnostics;

    /// <summary>
    ///   Album entity class
    /// </summary>
    /// <remarks>
    ///   This class represents an album instance. Default values for member fields:
    ///   <c>Id = Int.MinValue</c>
    ///   <c>ReleaseYear = 0</c>
    ///   <c>Name = "Unknown"</c>
    /// </remarks>
    [Serializable]
    [DebuggerDisplay("Name={_name}, Year={_releaseYear}")]
    public class Album
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string name;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int releaseYear;

        public Album()
        {
            Id = int.MinValue;
            releaseYear = 0;
            name = "Unknown";
        }

        public Album(int id, string name)
        {
            releaseYear = 0;
            Id = id;
            this.name = name;
        }

        public Album(int id, string name, int releaseYear)
            : this(id, name)
        {
            this.releaseYear = releaseYear;
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value.Length > 255 ? value.Substring(0, 255) : value;
            }
        }

        public int Id { get; set; }

        public int ReleaseYear
        {
            get
            {
                return releaseYear;
            }

            set
            {
                releaseYear = value;
            }
        }
    }
}