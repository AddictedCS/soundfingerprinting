namespace SoundFingerprinting.Audio.NAudio
{
    using global::NAudio.Wave;

    public class NAudioTagService : ITagService
    {
        public TagInfo GetTagInfo(string pathToAudioFile)
        {
            using (var reader = new MediaFoundationReader(pathToAudioFile))
            {
                return new TagInfo 
                    {
                        Duration = reader.TotalTime.TotalSeconds,
                        Album = string.Empty,
                        AlbumArtist = string.Empty,
                        Artist = pathToAudioFile,
                        Composer = string.Empty,
                        Genre = string.Empty,
                        IsEmpty = false,
                        ISRC = string.Empty,
                        Title = pathToAudioFile,
                        Year = 0
                    };
            }
        }
    }
}
