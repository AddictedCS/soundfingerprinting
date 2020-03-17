namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    public static class HashedFingerprintExtensions
    {
        public static double QueryLength(this IEnumerable<HashedFingerprint> hashedFingerprints, FingerprintConfiguration configuration)
        {
            double startsAt = double.MaxValue, endsAt = double.MinValue;
            foreach (var hashedFingerprint in hashedFingerprints)
            {
                startsAt = System.Math.Min(startsAt, hashedFingerprint.StartsAt);
                endsAt = System.Math.Max(endsAt, hashedFingerprint.StartsAt);
            }

            return SubFingerprintsToSeconds.MatchLengthToSeconds(endsAt, startsAt, configuration.FingerprintLengthInSeconds); 
        }
    }
}