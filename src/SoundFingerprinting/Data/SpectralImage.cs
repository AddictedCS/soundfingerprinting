namespace SoundFingerprinting.Data
{
    public class SpectralImage
    {
        /// <summary>
        /// Gets or sets spectral image
        /// </summary>
        public float[][] Image { get; set; }

        /// <summary>
        /// Gets or sets the sequence number of the spectral image. Possible values [1, N]
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the timestamp details of the spectral image. Possible values [0, N]
        /// </summary>
        public double Timestamp { get; set; }
    }
}
