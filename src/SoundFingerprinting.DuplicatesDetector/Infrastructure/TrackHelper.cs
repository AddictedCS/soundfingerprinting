namespace SoundFingerprinting.DuplicatesDetector.Infrastructure
{
    using System.IO;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.DuplicatesDetector.Model;

    public static class TrackHelper
    {
        /// <summary>
        ///   Get track samples
        /// </summary>
        /// <param name = "track">Track from which to gather samples</param>
        /// <param name = "proxy">Proxy used in gathering samples</param>
        /// <param name = "sampleRate">Sample rate used in gathering samples</param>
        /// <param name = "secondsToRead">Milliseconds to gather</param>
        /// <param name = "startAtSecond">Starting millisecond</param>
        /// <returns>Music samples</returns>
        public static float[] GetTrackSamples(Track track, IAudioService proxy, int sampleRate, int secondsToRead, int startAtSecond)
        {
            if (track == null || track.Path == null)
            {
                return null;
            }

            return proxy.ReadMonoFromFile(track.Path, sampleRate, secondsToRead, startAtSecond);
        }

        public static Track GetTrack(int mintracklen, int maxtracklen, string filename, ITagService tagService)
        {
            TagInfo tags = tagService.GetTagInfo(filename); // get file tags
            string artist, title, isrc, album;
            double duration;
            int year;
            if (tags == null)
            {
                /*The song does not contain any tags*/
                artist = "Unknown Artist";
                title = "Unknown Title";
                isrc = "Uknown ISRC";
                album = "Uknown Album";
                duration = new FileInfo(filename).Length;
                year = 0;
            }
            else
            {
                /*The song contains related tags*/
                artist = tags.Artist;
                title = tags.Title;
                duration = tags.Duration;
                year = tags.Year;
                isrc = tags.ISRC;
                album = tags.Album;
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

            return new Track(Path.GetFullPath(filename), isrc, artist, title, album, year, (int)duration);
        }
    }
}