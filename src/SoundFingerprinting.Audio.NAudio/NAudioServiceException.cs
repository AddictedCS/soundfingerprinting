namespace SoundFingerprinting.Audio.NAudio
{
    using System;

    public class NAudioServiceException : Exception
    {
        public NAudioServiceException(string errorMessage) : base(errorMessage)
        {
        }
    }
}
