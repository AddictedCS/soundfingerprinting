namespace SoundFingerprinting.Configuration
{
    using System;

    /// <summary>
    ///  Frequency range class.
    /// </summary>
    public class FrequencyRange
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="FrequencyRange"/> class.
        /// </summary>
        /// <param name="min">Min frequency value to consider.</param>
        /// <param name="max">Max frequency value to consider.</param>
        public FrequencyRange(ushort min, ushort max)
        {
            if (min >= max)
            {
                throw new ArgumentException("min value has to be less than max value", nameof(min));
            }
            
            Min = min;
            Max = max;
        }

        /// <summary>
        ///  Gets min frequency value to consider when generating audio fingerprints.
        ///  Measured in Hertz.
        /// </summary>
        public ushort Min { get; }

        /// <summary>
        ///  Gets max frequency value to consider when generating audio fingerprints.
        ///  Measured in Hertz.
        /// </summary>
        public ushort Max { get; }
    }
}