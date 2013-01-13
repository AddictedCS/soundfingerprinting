namespace Soundfingerprinting.DuplicatesDetector.Infrastructure
{
    using System;
    using System.IO;

    using Soundfingerprinting.Audio.Models;
    using Soundfingerprinting.Audio.Services;
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
        /// <returns></returns>
        public static float[] GetTrackSamples(
            Track track, IAudioService proxy, int sampleRate, int milliseconds, int startmilliseconds)
        {
            if (track == null || track.Path == null) return null;
            //read 5512 Hz, Mono, PCM, with a specific audioService
            return proxy.ReadMonoFromFile(track.Path, sampleRate, milliseconds, startmilliseconds);
        }

        /// <summary>
        ///   Get track info from the filename
        /// </summary>
        /// <param name = "mintracklen">Min track length</param>
        /// <param name = "maxtracklen">Max track length</param>
        /// <param name = "filename">Filename from which to extract the requested info</param>
        /// <param name = "audioService">Audio audioService to read tags</param>
        /// <returns>Track to be analyzed further / null if the track is not eligible</returns>
        public static Track GetTrackInfo(int mintracklen, int maxtracklen, string filename, ITagService tagService)
        {
            TagInfo tags = tagService.GetTagInfo(filename); //get file tags
            string artist, title;
            double duration;
            if (tags == null)
            {
                /*The song does not contain any tags*/
                artist = "Unknown";
                title = "Unknown";
                duration = 60;
            }
            else
            {
                /*The song contains related tags*/
                artist = tags.Artist;
                title = tags.Title;
                duration = tags.Duration;
            }

            if (String.IsNullOrEmpty(artist)) /*assign a name to music files that don't have tags*/
            {
                artist = "Unknown";
            }

            if (String.IsNullOrEmpty(title)) /*assign a title to music files that don't have tags*/
            {
                title = "Unknown";
            }

            if (duration < mintracklen || duration > maxtracklen) /*check the duration of a music file*/
            {
                return null;
            }
            Track track = new Track
                { Artist = artist, Title = title, TrackLength = duration, Path = Path.GetFullPath(filename) };
            return track;
        }
    }
}