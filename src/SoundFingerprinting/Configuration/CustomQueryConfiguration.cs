namespace SoundFingerprinting.Configuration
{
    using System;

    public class CustomQueryConfiguration : DefaultQueryConfiguration
    {
        public new int ThresholdVotes
        {
            get
            {
                return base.ThresholdVotes;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("ThresholdVotes cannot be less than 0", "value");
                }

                base.ThresholdVotes = value;
            }
        }

        public new int MaximumNumberOfTracksToReturnAsResult
        {
            get
            {
                return base.MaximumNumberOfTracksToReturnAsResult;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("MaximumNumberOfTracksToReturnAsResult cannot be less or equal to 0", "value");
                }

                base.MaximumNumberOfTracksToReturnAsResult = value;
            }
        }
    }
}
