namespace SoundFingerprinting.DAO;

using System;

/// <summary>
///  Model reference max value exceeded exception
/// </summary>
public class ModelReferenceMaxAllowedValueExceededException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelReferenceMaxAllowedValueExceededException"/> class.
    /// </summary>
    /// <param name="value">Value</param>
    public ModelReferenceMaxAllowedValueExceededException(long value)
    {
        Value = value;
    }

    /// <summary>
    ///  Gets model reference value that exceeded max allowed value;
    /// </summary>
    public long Value { get; }
}