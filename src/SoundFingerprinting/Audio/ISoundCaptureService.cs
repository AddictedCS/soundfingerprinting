namespace SoundFingerprinting.Audio
{
    /// <summary>
    ///  Sound capture service
    /// </summary>
    public interface ISoundCaptureService
    {
        /// <summary>
        /// Read mono samples from default recording device
        /// </summary>
        /// <param name="sampleRate">Target sample rate</param>
        /// <param name="secondsToRecord">Seconds to record</param>
        /// <returns>32 bit sample array, sampled at target rate</returns>
        float[] ReadMonoSamples(int sampleRate, int secondsToRecord);
    }
}