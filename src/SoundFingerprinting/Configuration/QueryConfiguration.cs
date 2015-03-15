namespace SoundFingerprinting.Configuration
{
    using System;

    public class QueryConfiguration
    {
        public static readonly QueryConfiguration Default = new DefaultQueryConfiguration();
     
        private int thresholdVotes;
        private int maximumNumberOfTracksToReturnAsResult;

        public int ThresholdVotes
        {
            get
            {
                return thresholdVotes;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("ThresholdVotes cannot be less than 0", "value");
                }

                thresholdVotes = value;
            }
        }

        public int MaximumNumberOfTracksToReturnAsResult
        {
            get
            {
                return maximumNumberOfTracksToReturnAsResult;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("MaximumNumberOfTracksToReturnAsResult cannot be less or equal to 0", "value");
                }

                maximumNumberOfTracksToReturnAsResult = value;
            }
        }

        public string TrackGroupId { get; set; }
    }
}