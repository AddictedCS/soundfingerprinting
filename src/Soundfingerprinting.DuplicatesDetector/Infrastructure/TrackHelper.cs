namespace Soundfingerprinting.DuplicatesDetector.Infrastructure
{
    using System.IO;

    using Soundfingerprinting.Audio;
    using Soundfingerprinting.DuplicatesDetector.Model;

    public static class TrackHelper
    {
        /// <summary>
        ///   Get track samples
        /// </summary>
        /// <param name = "track">Track from which to gather samples</param>
        /// <param name = "proxy">Proxy used in gathering samples</param>
        /// <param name = "sampleRate">Sample rate used in gathering samples</param>
        /// <param name = "milliseconds">Milliseconds to gather</param>
        /// <param name = "startmilliseconds">Starting millisecond</param>
        /// <returns>Music samples</returns>
        public static float[] GetTrackSamples(Track track, IAudioService proxy, int sampleRate, int milliseconds, int startmilliseconds)
        {
            if (track == null || track.Path == null)
            {
                return null;
            }

            return proxy.ReadMonoFromFile(track.Path, sampleRate, milliseconds, startmilliseconds);
        }

        public static Track GetTrack(int mintracklen, int maxtracklen, string filename, ITagService tagService)
        {
            TagInfo tags = tagService.GetTagInfo(filename); // get file tags
            string artist, title;
            double duration;
            if (tags == null)
            {
                /*The song does not contain any tags*/
                artist = "Unknown Artist";
                title = "Unknown Title";
                duration = new FileInfo(filename).Length;
            }
            else
            {
                /*The song contains related tags*/
                artist = tags.Artist;
                title = tags.Title;
                duration = tags.Duration;
            }

            /*assign a name to music files that don't have tags*/
            if (string.IsNullOrEmpty(artist))
            {
                artist = "Unknown Artist";
            }

            /*assign a title to music files that don't have tags*/
            if (string.IsNullOrEmpty(title))
            {
                title = "Unknown Title";
            }

            /*check the duration of a music file*/
            if (duration < mintracklen || duration > maxtracklen)
            {
                return null;
            }

            Track track = new Track(artist, title, Path.GetFullPath(filename), duration);
            return track;
        }
    }
}