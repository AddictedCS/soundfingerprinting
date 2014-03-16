namespace SoundFingerprinting.Audio.Bass
{
    using System;

    internal class BassAudioServiceException : Exception
    {
        public BassAudioServiceException(string errorMessage)
            : base(errorMessage)
        {
        }
    }
}
