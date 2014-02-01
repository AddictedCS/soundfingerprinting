namespace SoundFingerprinting.DuplicatesDetector.Infrastructure
{
    using System.IO;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Data;

    public class TrackHelper
    {
        private readonly ITagService tagService;

        private readonly IAudioService audioService;

        public TrackHelper(ITagService tagService, IAudioService audioService)
        {
            this.tagService = tagService;
            this.audioService = audioService;
        }

        public float[] GetTrackSamples(TrackData track, int sampleRate, int secondsToRead, int startAtSecond)
        {
            if (track == null || track.Album == null)
            {
                return null;
            }

            return audioService.ReadMonoFromFile(track.Album, sampleRate, secondsToRead, startAtSecond);
        }

        public TrackData GetTrack(int mintracklen, int maxtracklen, string filename)
        {
            TagInfo tags = tagService.GetTagInfo(filename); // get file tags
            string artist, title, isrc;
            double duration;
            int year;
            if (tags == null)
            {
                /*The song does not contain any tags*/
                artist = "Unknown Artist";
                title = "Unknown Title";
                isrc = "Uknown ISRC";
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

            return new TrackData(isrc, artist, title, Path.GetFullPath(filename), year, (int)duration);
        }
    }
}