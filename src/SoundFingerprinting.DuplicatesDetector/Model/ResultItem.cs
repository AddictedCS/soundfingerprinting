namespace SoundFingerprinting.DuplicatesDetector.Model
{
    public class ResultItem
    {
        private readonly Track track;

        public ResultItem(int setId, Track track)
        {
            SetId = setId;
            this.track = track;
        }

        public int SetId { get; private set; }

        public string Title
        {
            get { return track.Title; }
        }

        public string Artist
        {
            get { return track.Artist; }
        }

        public string FileName
        {
            get { return System.IO.Path.GetFileName(track.Path); }
        }

        public string Path
        {
            get { return track.Path; }
        }

        public double TrackLength
        {
            get { return track.TrackLengthSec; }
        }
    }
}