namespace Soundfingerprinting.Audio.Services
{
    using Soundfingerprinting.Audio.Models;

    public interface ITagService
    {
        TagInfo GetTagInfo(string pathToAudioFile);
    }
}