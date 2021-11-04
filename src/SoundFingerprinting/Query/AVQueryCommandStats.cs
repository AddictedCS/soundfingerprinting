namespace SoundFingerprinting.Query
{
    public class AVQueryCommandStats
    {
        public AVQueryCommandStats(QueryCommandStats? audio, QueryCommandStats? video)
        {
            Audio = audio;
            Video = video;
        }

        public QueryCommandStats? Audio { get; }

        public QueryCommandStats? Video { get; }

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

        public AVQueryCommandStats WithFingerprintingDurationMilliseconds(long audioFingerprinting, long videoFingerprinting)
        {
            var audio = Audio?.WithFingerprintingDurationMilliseconds(audioFingerprinting);
            var video = Video?.WithFingerprintingDurationMilliseconds(videoFingerprinting);
            return new AVQueryCommandStats(audio, video);
        }
    }
}