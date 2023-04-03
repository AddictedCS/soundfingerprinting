namespace SoundFingerprinting.Query
{
    /// <summary>
    ///  Class that contains all the information related to audio/video query command statistics.
    /// </summary>
    public class AVQueryCommandStats
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="AVQueryCommandStats"/> class.
        /// </summary>
        /// <param name="audio">Audio query command statistics.</param>
        /// <param name="video">Video query command statistics.</param>
        public AVQueryCommandStats(QueryCommandStats? audio, QueryCommandStats? video)
        {
            Audio = audio;
            Video = video;
        }

        /// <summary>
        ///  Gets audio query command statistics.
        /// </summary>
        public QueryCommandStats? Audio { get; }

        /// <summary>
        ///  Gets video query command statistics.
        /// </summary>
        public QueryCommandStats? Video { get; }

        /// <summary>
        ///  Calculate sum of two query command statistics.
        /// </summary>
        /// <param name="stats">An instance of <see cref="AVQueryCommandStats"/>.</param>
        /// <returns>Summed query command statistics.</returns>
        public AVQueryCommandStats Sum(AVQueryCommandStats? stats)
        {
            if (stats == null)
            {
                return this;
            }

            var audio = Audio?.Sum(stats.Audio) ?? stats.Audio;
            var video = Video?.Sum(stats.Video) ?? stats.Video;
            return new AVQueryCommandStats(audio, video);
        }

        /// <summary>
        ///  With audio fingerprinting duration in milliseconds.
        /// </summary>
        /// <param name="audioFingerprinting">Audio fingerprinting time (measured in milliseconds).</param>
        /// <param name="videoFingerprinting">Video fingerprinting time (measured in milliseconds).</param>
        /// <returns>A new instance of <see cref="AVQueryCommandStats"/>.</returns>
        public AVQueryCommandStats WithFingerprintingDurationMilliseconds(long audioFingerprinting, long videoFingerprinting)
        {
            var audio = Audio?.WithFingerprintingDurationMilliseconds(audioFingerprinting);
            var video = Video?.WithFingerprintingDurationMilliseconds(videoFingerprinting);
            return new AVQueryCommandStats(audio, video);
        }
    }
}