namespace SoundFingerprinting.Configuration
{
    using System;

    public abstract class HashingConfig
    {
        public static readonly HashingConfig Default = new DefaultHashingConfig();

        private int numberOfLSHTables;

        private int numberOfMinHashesPerTable;

        /// <summary>
        /// Gets or sets the number of Locality Sensitive tables to split into
        /// </summary>
        public int NumberOfLSHTables
        {
            get
            {
                return numberOfLSHTables;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("NumberOfLSHTables can't be negative or equal to 0", "value");
                }

                numberOfLSHTables = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of Min Hashes per table (hash bucket)
        /// </summary>
        public int NumberOfMinHashesPerTable
        {
            get
            {
                return numberOfMinHashesPerTable;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("NumberOfMinHashesPerTable can't be negative or equal to 0", "value");
                }

                numberOfMinHashesPerTable = value;
            }
        }
    }
}