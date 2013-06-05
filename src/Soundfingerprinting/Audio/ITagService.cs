namespace Soundfingerprinting.Audio
{
    public interface ITagService
    {
        TagInfo GetTagInfo(string pathToAudioFile);
    }
}