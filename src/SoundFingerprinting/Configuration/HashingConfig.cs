namespace SoundFingerprinting.Configuration
{
    public abstract class HashingConfig
    {
        public static readonly HashingConfig Default = new DefaultHashingConfig();

        /// <summary>
        /// Gets or sets the number of Locality Sensitive tables to split into
        /// </summary>
        public int NumberOfLSHTables { get; set; }

        /// <summary>
        /// Gets or sets the number of Min Hashes per table (hash bucket)
        /// </summary>
        public int NumberOfMinHashesPerTable { get; set; }
    }
}