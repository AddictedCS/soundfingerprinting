namespace SoundFingerprinting.LCS;

/// <summary>
///  Time segment representing a time interval.
/// </summary>
/// <param name="StartsAt">Starts at (measured in seconds)</param>
/// <param name="EndsAt">Ends at (measured in seconds)</param>
public record TimeSegment(double StartsAt, double EndsAt)
{
    /// <summary>
    ///  Gets start at index.
    /// </summary>
    public double StartsAt { get; } = StartsAt;

    /// <summary>
    ///  Gets ends at index.
    /// </summary>
    public double EndsAt { get; } = EndsAt;

    /// <summary>
    ///  Gets length of the time segment.
    /// </summary>
    public double TotalSeconds => EndsAt - StartsAt;
}