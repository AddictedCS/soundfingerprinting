namespace Soundfingerprinting.AudioProxies
{
    public interface ITagService
    {
        TagInfo GetTagInfo(string pathToAudioFile);
    }

    public class TagService : ITagService
    {
        public TagInfo GetTagInfo(string pathToAudioFile)
        {
            throw new System.NotImplementedException();
        }
    }
}