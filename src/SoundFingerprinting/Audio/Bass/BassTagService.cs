namespace SoundFingerprinting.Audio.Bass
{
    using SoundFingerprinting.Infrastructure;

    public class BassTagService : ITagService
    {
        private readonly IBassServiceProxy bassServiceProxy;

        public BassTagService()
            : this(DependencyResolver.Current.Get<IBassServiceProxy>())
        {
        }

        private BassTagService(IBassServiceProxy bassServiceProxy)
        {
            this.bassServiceProxy = bassServiceProxy;
        }

        public TagInfo GetTagInfo(string pathToAudioFile)
        {
            var tags = bassServiceProxy.GetTagsFromFile(pathToAudioFile);
            if (tags == null)
            {
                return new TagInfo { IsEmpty = true };
            }

            int year;
            int.TryParse(tags.year, out year);
            TagInfo tagInfo = new TagInfo
            {
                Duration = tags.duration,
                Album = tags.album,
                Artist = tags.artist,
                Title = tags.title,
                AlbumArtist = tags.albumartist,
                Genre = tags.genre,
                Year = year,
                Composer = tags.composer,
                ISRC = tags.isrc
            };

            return tagInfo;
        }
    }
}
