namespace Soundfingerprinting.DbStorage.Entities
{
    public class Track
    {
        private string artist;

        private string title;

        private int trackLength;

        public Track()
        {
        }

        public Track(int trackId, string artist, string title, int albumId)
        {
            Id = trackId;
            Artist = artist;
            Title = title;
            AlbumId = albumId;
        }

        public Track(int trackId, string artist, string title, int albumId, int trackLength)
            : this(trackId, artist, title, albumId)
        {
            TrackLength = trackLength;
        }

        public int Id { get; set; }

        public string Artist
        {
            get
            {
                return artist;
            }

            set
            {
                if (value.Length > 255)
                {
                    throw new FingerprintEntityException(
                        "Artist's length cannot exceed a predefined value. Check the documentation");
                }

                artist = value;
            }
        }

        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                if (value.Length > 255)
                {
                    throw new FingerprintEntityException(
                        "Title's length cannot exceed a predefined value. Check the documentation");
                }

                title = value;
            }
        }

        public int AlbumId { get; set; }

        public int TrackLength
        {
            get
            {
                return trackLength;
            }

            set
            {
                if (value < 0)
                {
                    throw new FingerprintEntityException("Track's Length cannot be less than 0");
                }

                trackLength = value;
            }
        }
    }
}