namespace SoundFingerprinting.Builder
{
    using System;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Command;

    /// <summary>
    ///  Extensions for configuring query sources.
    /// </summary>
    public static class QuerySourceExtensions
    {
        /// <summary>
        ///   Builds query fingerprints from audio samples captured with a known playback-speed change.
        /// </summary>
        /// <param name="source">Query source.</param>
        /// <param name="audioSamples">Audio samples to build the fingerprints from.</param>
        /// <param name="sourcePlaybackSpeedPercentage">
        ///   Playback-speed change applied to the source. For example, pass <c>4</c> when the source
        ///   was played 4% faster, or <c>-4</c> when it was played 4% slower.
        /// </param>
        /// <returns>Configuration selector.</returns>
        /// <remarks>
        ///  This compensates vinyl-style pitch control where tempo and pitch change together.
        ///  It does not compensate pitch-only processing such as key lock.
        /// </remarks>
        public static IWithQueryConfiguration From(
            this IQuerySource source,
            AudioSamples audioSamples,
            double sourcePlaybackSpeedPercentage)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var compensated = PlaybackSpeedAudioSamplesCompensator.Compensate(
                audioSamples,
                sourcePlaybackSpeedPercentage);
            return source.From(compensated);
        }
    }
}
