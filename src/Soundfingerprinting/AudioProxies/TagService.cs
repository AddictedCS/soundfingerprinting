namespace Soundfingerprinting.AudioProxies
{
    using Un4seen.Bass.AddOn.Tags;

    public class TagService : ITagService
    {
        public TagInfo GetTagInfo(string pathToAudioFile)
        {
            TAG_INFO tags = BassTags.BASS_TAG_GetFromFile(pathToAudioFile);
            TagInfo tag = new TagInfo()
                {
                    Duration = tags.duration,
                    Album = tags.album,
                    Artist = tags.artist,
                    Title = tags.title,
                    AlbumArtist = tags.albumartist,
                    Genre = tags.genre
                };

            return tag;
        }
    }
}