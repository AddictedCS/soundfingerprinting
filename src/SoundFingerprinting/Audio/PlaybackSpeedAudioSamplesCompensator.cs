namespace SoundFingerprinting.Audio
{
    using System;

    internal static class PlaybackSpeedAudioSamplesCompensator
    {
        public static AudioSamples Compensate(AudioSamples audioSamples, double sourcePlaybackSpeedPercentage)
        {
            if (audioSamples == null)
            {
                throw new ArgumentNullException(nameof(audioSamples));
            }

            if (double.IsNaN(sourcePlaybackSpeedPercentage) || double.IsInfinity(sourcePlaybackSpeedPercentage))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sourcePlaybackSpeedPercentage),
                    "Playback-speed percentage must be a finite number.");
            }

            double sourcePlaybackRate = 1d + (sourcePlaybackSpeedPercentage / 100d);
            if (sourcePlaybackRate <= 0d || double.IsInfinity(sourcePlaybackRate))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sourcePlaybackSpeedPercentage),
                    "Playback-speed percentage must produce a finite playback rate greater than zero.");
            }

            if (Math.Abs(sourcePlaybackSpeedPercentage) < double.Epsilon || audioSamples.Samples.Length < 2)
            {
                return audioSamples;
            }

            double compensationRate = 1d / sourcePlaybackRate;
            double requestedOutputLength = Math.Floor((audioSamples.Samples.Length - 1) / compensationRate) + 1;
            if (requestedOutputLength > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sourcePlaybackSpeedPercentage),
                    "Playback-speed compensation would create too many audio samples.");
            }

            int outputLength = Math.Max(2, (int)requestedOutputLength);
            float[] output = new float[outputLength];

            for (int outputIndex = 0; outputIndex < output.Length; outputIndex++)
            {
                double sourcePosition = outputIndex * compensationRate;
                int lowerIndex = Math.Min((int)sourcePosition, audioSamples.Samples.Length - 1);
                int upperIndex = Math.Min(lowerIndex + 1, audioSamples.Samples.Length - 1);
                double fraction = sourcePosition - lowerIndex;
                output[outputIndex] = (float)(
                    audioSamples.Samples[lowerIndex] +
                    ((audioSamples.Samples[upperIndex] - audioSamples.Samples[lowerIndex]) * fraction));
            }

            return new AudioSamples(
                output,
                audioSamples.Origin,
                audioSamples.SampleRate,
                audioSamples.RelativeTo,
                audioSamples.TimeOffset);
        }
    }
}
