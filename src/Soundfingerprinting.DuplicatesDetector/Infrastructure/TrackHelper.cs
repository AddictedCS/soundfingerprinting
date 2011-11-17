// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.IO;
using Soundfingerprinting.AudioProxies;
using Soundfingerprinting.DuplicatesDetector.Model;
using Un4seen.Bass.AddOn.Tags;

namespace Soundfingerprinting.DuplicatesDetector.Infrastructure
{
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
        public static float[] GetTrackSamples(Track track, BassProxy proxy, int sampleRate, int milliseconds, int startmilliseconds)
        {
            if (track == null || track.Path == null)
                return null;
            //read 5512 Hz, Mono, PCM, with a specific proxy
            return proxy.ReadMonoFromFile(track.Path, sampleRate, milliseconds, startmilliseconds);
        }

        /// <summary>
        ///   Get track info from the filename
        /// </summary>
        /// <param name = "mintracklen">Min track length</param>
        /// <param name = "maxtracklen">Max track length</param>
        /// <param name = "filename">Filename from which to extract the requested info</param>
        /// <param name = "proxy">Audio proxy to read tags</param>
        /// <returns>Track to be analyzed further / null if the track is not eligible</returns>
        public static Track GetTrackInfo(int mintracklen, int maxtracklen, string filename, BassProxy proxy)
        {
            TAG_INFO tags = proxy.GetTagInfoFromFile(filename); //get file tags
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
                artist = tags.artist;
                title = tags.title;
                duration = tags.duration;
            }
            if (String.IsNullOrEmpty(artist)) /*assign a name to music files that don't have tags*/
                artist = "Unknown";
            if (String.IsNullOrEmpty(title)) /*assign a title to music files that don't have tags*/
                title = "Unknown";
            if (duration < mintracklen || duration > maxtracklen) /*check the duration of a music file*/
                return null;
            Track track = new Track
                          {
                              Artist = artist,
                              Title = title,
                              TrackLength = duration,
                              Path = Path.GetFullPath(filename)
                          };
            return track;
        }
    }
}