namespace SoundFingerprinting.Audio
{
    using System;

    [Serializable]
    public class AudioServiceException : Exception
    {
        public AudioServiceException(string errorMessage) : base(errorMessage)
        {
        }
    }
}
