namespace SoundFingerprinting.Query
{
    internal class SubFingerprintsToSeconds
    {
        public static double MatchLengthToSeconds(double endsAt, double startsAt, double fingerprintLengthInSeconds)
        {
            return endsAt - startsAt + fingerprintLengthInSeconds;
        }

        public static double GapLengthToSeconds(double endsAt, double startsAt, double fingerprintLengthInSeconds)
        {
            return endsAt - (startsAt + fingerprintLengthInSeconds);
        }
    }
}
