namespace SoundFingerprinting.Audio;

/// <summary>
///  Per-second spectral flatness measure and relative power.
/// </summary>
public readonly struct SpectralSecond
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="SpectralSecond"/> struct.
    /// </summary>
    /// <param name="sfm">Spectral flatness measure for this second, bounded in <c>[0, 1]</c>.</param>
    /// <param name="power">Mean spectrum power for this second, scaled relative to the audio max (<c>[0, 1]</c>).</param>
    public SpectralSecond(double sfm, double power)
    {
        Sfm = sfm;
        Power = power;
    }

    /// <summary>
    ///  Gets spectral flatness measure for this second. <c>0</c> = tonal, <c>1</c> = broadband.
    /// </summary>
    public double Sfm { get; }

    /// <summary>
    ///  Gets the mean spectrum power for this second, scaled relative to the maximum bucket power in the same audio.
    /// </summary>
    public double Power { get; }
}