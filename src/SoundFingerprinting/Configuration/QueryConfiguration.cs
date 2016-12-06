namespace SoundFingerprinting.Configuration
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///    Configuration options used during querying the data source
    /// </summary>
    public abstract class QueryConfiguration
    {
        private int thresholdVotes;
        private int maxTracksToReturn;

        /// <summary>
        ///   Gets or sets vote count for a track to be considered a potential match (i.e. [1; 25]).
        /// </summary>
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

        /// <summary>
        ///  Gets or sets maximum number of tracks to return out of all analyzed candidates
        /// </summary>
        public int MaxTracksToReturn
        {
            get
            {
                return this.maxTracksToReturn;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("MaxTracksToReturn cannot be less or equal to 0", "value");
                }

                this.maxTracksToReturn = value;
            }
        }

        /// <summary>
        ///  Gets or sets list of clusters to consider when querying the datasource for potential candidates
        /// </summary>
        public IEnumerable<string> Clusters { get; set; }

        /// <summary>
        /// Gets or sets fingerprint configuration used during querying. This field will be used later on for internal purposes. 
        /// It doesnt have to be exposed to the outside framework users.
        /// </summary>
        internal FingerprintConfiguration FingerprintConfiguration { get; set; }
    }
}