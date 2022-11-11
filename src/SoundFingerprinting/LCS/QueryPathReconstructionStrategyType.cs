namespace SoundFingerprinting.LCS;

/// <summary>
///  Query path reconstruction strategy.
/// </summary>
public enum QueryPathReconstructionStrategyType
{
    /// <summary>
    ///  Legacy query path reconstruction strategy
    /// </summary>
    /// <remarks>
    ///  Used prior to v8.15.0.
    /// </remarks>
    Legacy = 1,
    
    /// <summary>
    ///  Single best path reconstruction strategy
    /// </summary>
    SingleBestPath = 2,
    
    /// <summary>
    ///  Multiple best paths reconstruction strategies
    /// </summary>
    MultipleBestPaths = 3
}