namespace SoundFingerprinting.Configuration
{
    /// <summary>
    ///  Locality sensitive hashing algorithm configuration.
    /// </summary>
    /// <remarks>
    ///  Since we are not operating in geometric space, instead of making random projections (most used LSH scenario), we will apply random set permutations known as min-hashing as our LSH schema. <br/>
    ///  The configuration options for min-hash as our LSH schema are defined in this class.
    /// </remarks>
    public abstract class HashingConfig
    {
        /// <summary>
        /// Gets or sets the number of Locality Sensitive tables to split into.
        /// </summary>
        public int NumberOfLSHTables { get; set; }

        /// <summary>
        /// Gets or sets the number of Min Hashes per table (hash bucket).
        /// </summary>
        public int NumberOfMinHashesPerTable { get; set; }

        /// <summary>
        ///  Maximum number of hash buckets allowed per hash table.
        /// </summary>
        public int HashBuckets { get; set; }

        /// <summary>
        ///  Gets or sets the width of the image that is hashed.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        ///  Gets or sets the height of the image that is hashed.
        /// </summary>
        public int Height { get; set; }
    }
}