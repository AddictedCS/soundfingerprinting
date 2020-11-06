namespace SoundFingerprinting.Configuration
{
    using System.Collections.Generic;
    using SoundFingerprinting.DAO.Data;

    public interface IMetaFieldsFilter
    {
        /// <summary>
        ///  Checks if metadata passes both <b>yes</b> and <b>no</b> filters.
        /// </summary>
        /// <param name="metadata">Track metadata (see <see cref="TrackData.MetaFields"/>).</param>
        /// <param name="yesFilters">
        ///   Key-value pairs that metadata <b>MUST</b> contain in order to pass filtering.
        /// </param>
        /// <param name="noFilters">
        ///   Key-value pairs that metadata <b>MUST NOT</b> contain in order to pass filtering.
        /// </param>
        /// <returns>True if conditions are met, otherwise false.</returns>
        bool PassesFilters(IDictionary<string, string> metadata,
            IDictionary<string, string> yesFilters, 
            IDictionary<string, string> noFilters);
    }
}