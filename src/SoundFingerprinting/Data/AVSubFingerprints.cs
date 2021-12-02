namespace SoundFingerprinting.Data
{
    using System.Collections.Generic;
    using SoundFingerprinting.DAO.Data;

    /// <summary>
    ///  Class that holds information about audio/video fingerprints response from <see cref="IModelService"/>.
    /// </summary>
    public class AVSubFingerprints
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AVSubFingerprints"/> class.
        /// </summary>
        /// <param name="audio">Audio.</param>
        /// <param name="video">Video.</param>
        public AVSubFingerprints(IEnumerable<SubFingerprintData> audio, IEnumerable<SubFingerprintData> video)
        {
            Audio = audio;
            Video = video;
        }

        /// <summary>
        ///  Gets audio sub-fingerprints.
        /// </summary>
        public IEnumerable<SubFingerprintData> Audio { get; }

        /// <summary>
        ///  Gets video sub-fingerprints.
        /// </summary>
        public IEnumerable<SubFingerprintData> Video { get; }
    }
}