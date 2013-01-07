namespace Soundfingerprinting.AudioProxies
{
    public interface ITagService
    {
        TagInfo GetTagInfo(string pathToAudioFile);
    }
}