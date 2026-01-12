namespace SoundFingerprinting.Configuration;

/// <summary>
///  Specifies the logic used to combine multiple filter conditions.
/// </summary>
public enum FilterLogic
{
    /// <summary>
    ///  At least one condition must be met (default behavior).
    /// </summary>
    Or,
    
    /// <summary>
    ///  All specified conditions must be met.
    /// </summary>
    And
}
