namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.Configuration;

    internal class SubFingerprintsToSeconds
    {
        public static double AdjustLengthToSeconds(double endsAt, double startsAt, FingerprintConfiguration configuration)
        {
            return endsAt - startsAt + configuration.FingerprintLengthInSeconds;
        }
    }
}
