namespace SoundFingerprinting.Audio
{
    using System.Collections.Generic;

    /// <summary>
    ///  Service that allows extracting tag information.
    /// </summary>
    public interface ITagService
    {
        /// <summary>
        ///  Get tag info from the audio file.
        /// </summary>
        /// <param name="pathToAudioFile">Path to audio file (mp3)</param>
        /// <returns>An instance of </returns>
        TagInfo GetTagInfo(string pathToAudioFile);
        
        /// <summary>
        ///  Try to get tag by key from the audio file.
        /// </summary>
        /// <param name="pathToAudioFile">Path to audio file (mp3)</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <returns>True if successful, otherwise false.</returns>
        bool TryGetTagByKey(string pathToAudioFile, string key, out string? value);

        /// <summary>
        ///  Set tag by key in the audio file.
        /// </summary>
        /// <param name="pathToAudioFile">Path to audio file (mp3)</param>
        /// <param name="tags">Key/Values.</param>
        void SetTagByKey(string pathToAudioFile, IEnumerable<KeyValuePair<string, string>> tags);
    }
}