namespace SoundFingerprinting.Audio
{
    public interface ITagService
    {
        TagInfo GetTagInfo(string pathToAudioFile);
    }
}