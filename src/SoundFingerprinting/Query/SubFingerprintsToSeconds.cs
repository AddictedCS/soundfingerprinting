﻿namespace SoundFingerprinting.Query
{
    internal class SubFingerprintsToSeconds
    {
        public static double MatchLengthToSeconds(double endsAt, double startsAt, double fingerprintLengthInSeconds)
        {
            return (endsAt + fingerprintLengthInSeconds) - startsAt;
        }

        public static double GapLengthToSeconds(double endsAt, double startsAt, double fingerprintLengthInSeconds)
        {
            return System.Math.Max(0, endsAt - (startsAt + fingerprintLengthInSeconds));
        }
    }
}
