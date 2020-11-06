namespace SoundFingerprinting.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class MetaFieldsFilter : IMetaFieldsFilter
    {
        /// <inheritdoc />
        public bool PassesFilters(IDictionary<string, string> metadata, 
            IDictionary<string, string> yesFilters, 
            IDictionary<string, string> noFilters)
        {
            if (yesFilters.Any() && !ContainsMetaFields(metadata, yesFilters))
            {
                return false;
            }

            if (noFilters.Any() && ContainsMetaFields(metadata, noFilters))
            {
                return false;
            }

            return true;
        }
        
        private static bool ContainsMetaFields(IDictionary<string, string> metadata, IDictionary<string, string> filter)
        {
            return metadata
                .Join(filter, _ => _.Key, _ => _.Key, (a, b) => string.Equals(a.Value, b.Value, StringComparison.InvariantCulture))
                .Any(_ => _);
        }
    }
}