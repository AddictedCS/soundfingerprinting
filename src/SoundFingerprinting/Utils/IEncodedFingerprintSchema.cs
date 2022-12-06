namespace SoundFingerprinting.Utils
{
    /// <summary>
    ///  Class that represents an encoded fingerprint schema.
    /// </summary>
    public interface IEncodedFingerprintSchema
    {
        /// <summary>
        ///  Gets a flag indicating whether extracted fingerprint array has a true value (i.e., 1) set on given position.
        /// </summary>
        /// <param name="index">Index to check upon.</param>
        /// <returns>True if set, otherwise false.</returns>
        public bool this[int index]
        {
            get;
        }

        /// <summary>
        ///  Gets a value indicating whether fingerprint array contains any values or is just silence (for video fingerprints black frame).
        /// </summary>
        /// <returns>True if fingerprint is silence, otherwise false.</returns>
        bool IsSilence
        {
            get;
        }

        /// <summary>
        ///  Converts encoded fingerprint into bool array.
        /// </summary>
        /// <returns>Array of booleans</returns>
        bool[] ConvertToBooleans();
    }
}