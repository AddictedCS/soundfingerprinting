namespace SoundFingerprinting.Audio.Bass
{
    using System;

    public class BassException : Exception
    {
        public BassException(string errorMessage)
            : base(errorMessage)
        {
        }
    }
}
