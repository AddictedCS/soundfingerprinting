namespace SoundFingerprinting.Query
{
    internal class SubFingerprintsToSeconds
    {
        public static double AdjustLengthToSeconds(double endsAt, double startsAt, double fingerprintLengthInSeconds)
        {
            return endsAt - startsAt + fingerprintLengthInSeconds;
        }
    }
}
