namespace Soundfingerprinting.DbStorage.Entities
{
    using System;
    using System.Diagnostics;

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
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string name;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int releaseYear;

        public Album()
        {
            Id = int.MinValue;
            releaseYear = 1501; /*Check docs*/
            name = "Unknown";
        }

        public Album(int id, string name)
        {
            releaseYear = 1501; /*Check docs*/
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
                if (value.Length > 255 /*DB Size Item*/)
                {
                    throw new FingerprintEntityException(
                        "Name length cannot exceed a predefined value. Check the documentation");
                }

                name = value;
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
                if (value > 1500 && value < 2100)
                {
                    releaseYear = value;
                }
                else
                {
                    throw new FingerprintEntityException(
                        "ReleaseYear does not match a predefined range. Check the documentation");
                }
            }
        }
    }
}