namespace SoundFingerprinting.Dao.Entities
{
    using System;
    using System.Diagnostics;

    [Serializable]
    [DebuggerDisplay("Name={albumName}, Year={releaseYear}")]
    public class Album
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Album UnknownAlbumInstance = new UnknownAlbum();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string albumName;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int releaseYear;

        public Album()
        {
            // no op
        }

        public Album(string albumName)
        {
            this.albumName = albumName;
        }

        public Album(string albumName, int releaseYear)
            : this(albumName)
        {
            this.releaseYear = releaseYear;
        }

        public static Album UnknowAlbum
        {
            get
            {
                return UnknownAlbumInstance;
            }
        }

        public virtual string Name
        {
            get
            {
                return albumName;
            }

            set
            {
                albumName = value.Length > 255 ? value.Substring(0, 255) : value;
            }
        }

        public virtual int Id { get; set; }

        public virtual int ReleaseYear
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

        private class UnknownAlbum : Album
        {
            public override int Id
            {
                get
                {
                    return -1; // defined as well on database level (see DBScript file)
                }

                set
                {
                    throw new Exception("Unmodifyable album instance");
                }
            }

            public override string Name
            {
                get
                {
                    return "UNKNOWN";
                }

                set
                {
                    throw new Exception("Unmodifyable album instance");
                }
            }

            public override int ReleaseYear
            {
                get
                {
                    return 0;
                }

                set
                {
                    throw new Exception("Unmodifyable album instance");
                }
            }
        }
    }
}