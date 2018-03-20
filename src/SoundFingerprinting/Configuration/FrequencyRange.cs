namespace SoundFingerprinting.Configuration
{
    public class FrequencyRange
    {
        public FrequencyRange(ushort min, ushort max)
        {
            Min = min;
            Max = max;
        }

        public ushort Min { get; }

        public ushort Max { get; }
    }
}