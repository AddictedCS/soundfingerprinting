namespace SoundFingerprinting.Audio.Bass
{
    using System;

    public class AudioServiceException : Exception
    {
        public AudioServiceException(string errorMessage) : base(errorMessage)
        {
        }
    }
}
