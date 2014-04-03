namespace SoundFingerprinting.Audio.Bass
{
    using System;

    internal class BassPlayAudioFileServiceException : Exception
    {
        public BassPlayAudioFileServiceException(string errorMessage) : base(errorMessage)
        {
        }
    }
}
