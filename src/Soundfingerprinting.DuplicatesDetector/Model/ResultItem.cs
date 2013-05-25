namespace Soundfingerprinting.DuplicatesDetector.Model
{
    /// <summary>
    ///   Result item
    /// </summary>
    public class ResultItem
    {
        /// <summary>
        ///   Result track
        /// </summary>
        private readonly Track _track;

        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name = "setId">Set id to which the track belongs</param>
        /// <param name = "track">Track</param>
        public ResultItem(int setId, Track track)
        {
            SetId = setId;
            _track = track;
        }

        /// <summary>
        ///   Set id to which the track belongs
        /// </summary>
        public int SetId { get; private set; }

        /// <summary>
        ///   Title
        /// </summary>
        public string Title
        {
            get { return _track.Title; }
        }

        /// <summary>
        ///   Artist
        /// </summary>
        public string Artist
        {
            get { return _track.Artist; }
        }

        /// <summary>
        ///   Filename
        /// </summary>
        public string FileName
        {
            get { return System.IO.Path.GetFileName(_track.Path); }
        }

        /// <summary>
        ///   Path
        /// </summary>
        public string Path
        {
            get { return _track.Path; }
        }

        /// <summary>
        ///   Track length
        /// </summary>
        public double TrackLength
        {
            get { return _track.TrackLength; }
        }
    }
}