namespace SoundFingerprinting.Configuration
{
    using System;

    /// <summary>
    ///  What a realtime query does when the track position jumps ahead of the stream across a query seam.
    /// </summary>
    public enum SeamJumpKind
    {
        /// <summary>
        ///  The match continues whatever the jump (the historical behavior).
        /// </summary>
        Stitch,

        /// <summary>
        ///  The source skipped (an audio/video player seeking): the match continues and the skipped track time is booked into the query length.
        /// </summary>
        SourceSkip,

        /// <summary>
        ///  The source is continuous: a jump up to the ceiling is an edit or a glitch and the match continues, a larger jump is another occurrence of the audio and starts a new match.
        /// </summary>
        NewOccurrence,
    }

    /// <summary>
    ///  Policy for the one decision a realtime query takes at a seam between consecutive queries: the track position jumped ahead of the stream, is this still the same playing of the track?
    /// </summary>
    /// <remarks>
    ///  A track that repeats its own audio (a program that contains its spot several times) matches consecutive queries at copies that are minutes apart.
    ///  Stitching them yields one match whose track span covers minutes of content that never aired. <see cref="NewOccurrenceAbove"/> starts a new match instead. <br />
    ///  Jumps backwards are not governed by the policy: a query that re-matches audio behind the accumulated coverage always starts a new match.
    /// </remarks>
    public sealed class SeamJumpPolicy
    {
        private SeamJumpPolicy(SeamJumpKind kind, double ceilingSeconds)
        {
            Kind = kind;
            CeilingSeconds = ceilingSeconds;
        }

        /// <summary>
        ///  Gets the policy that continues the match whatever the jump (default).
        /// </summary>
        public static SeamJumpPolicy Stitch { get; } = new SeamJumpPolicy(SeamJumpKind.Stitch, double.PositiveInfinity);

        /// <summary>
        ///  Gets the policy for a source that seeks: the match continues and the skipped track time is booked into the query length.
        /// </summary>
        public static SeamJumpPolicy SourceSkip { get; } = new SeamJumpPolicy(SeamJumpKind.SourceSkip, double.PositiveInfinity);

        /// <summary>
        ///  Gets the kind of the policy.
        /// </summary>
        public SeamJumpKind Kind { get; }

        /// <summary>
        ///  Gets the longest jump (in seconds) that still continues the match; <see cref="double.PositiveInfinity"/> unless the kind is <see cref="SeamJumpKind.NewOccurrence"/>.
        /// </summary>
        public double CeilingSeconds { get; }

        /// <summary>
        ///  Creates the policy for a continuous source: a jump above the ceiling starts a new match.
        /// </summary>
        /// <param name="ceilingSeconds">Longest jump (in seconds) that is still an edit or a glitch of the same playing. The permitted gap is added as alignment slack.</param>
        /// <returns>The policy.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The ceiling is negative, NaN or infinite.</exception>
        public static SeamJumpPolicy NewOccurrenceAbove(double ceilingSeconds)
        {
            if (double.IsNaN(ceilingSeconds) || double.IsInfinity(ceilingSeconds) || ceilingSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ceilingSeconds), ceilingSeconds, "The ceiling must be a finite number of seconds, zero or more.");
            }

            return new SeamJumpPolicy(SeamJumpKind.NewOccurrence, ceilingSeconds);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Kind == SeamJumpKind.NewOccurrence ? $"{Kind} above {CeilingSeconds:0.##}s" : Kind.ToString();
        }
    }
}
