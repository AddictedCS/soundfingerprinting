namespace SoundFingerprinting.Configuration.Frames;

using System.Collections.Generic;
using SoundFingerprinting.Data;

/// <summary>
///  Frame normalization chain.
/// </summary>
public class FrameNormalizationChain : IFrameNormalization
{
    private readonly IEnumerable<IFrameNormalization> normalizations;

    /// <summary>
    /// Initializes a new instance of the <see cref="FrameNormalizationChain"/> class.
    /// </summary>
    /// <param name="normalizations">Normalizations to apply.</param>
    public FrameNormalizationChain(IEnumerable<IFrameNormalization> normalizations)
    {
        this.normalizations = normalizations;
    }

    /// <inheritdoc cref="IFrameNormalization.Normalize"/>
    public IEnumerable<Frame> Normalize(IEnumerable<Frame> frames)
    {
        var normalized = frames;
        foreach (var normalization in normalizations)
        {
            normalized = normalization.Normalize(normalized);
        }

        return normalized;
    }
}