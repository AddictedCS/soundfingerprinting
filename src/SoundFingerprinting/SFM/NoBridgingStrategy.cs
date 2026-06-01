namespace SoundFingerprinting.SFM;

using System.Collections.Generic;

/// <summary>
///  Default <see cref="ISfmMatchStrategy"/> — emits no synthetics.
///  Behavior identical to the pre-bridging pipeline.
/// </summary>
public sealed class NoBridgingStrategy : ISfmMatchStrategy
{
    /// <summary>
    ///  Gets singleton instance.
    /// </summary>
    public static NoBridgingStrategy Default { get; } = new ();

    /// <inheritdoc />
    public double MaxQueryRelativeBridge => 0;

    /// <inheritdoc />
    public double MaxAbsoluteBridgeSeconds => double.PositiveInfinity;

    /// <inheritdoc />
    public IEnumerable<SyntheticCandidate> GenerateCandidates(SyntheticMatchContext context)
    {
        return [];
    }
}