namespace SoundFingerprinting;

/// <summary>
///  Describes the outcome of an <see cref="IModelService.Insert"/> call.
/// </summary>
public enum InsertOutcome
{
    /// <summary>
    ///  Outcome is unknown. This is the wire default produced when a new client talks to an older server
    ///  that does not report an outcome; it is never emitted by an up-to-date server and is treated as persisted.
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///  A new track and its hashes were persisted.
    /// </summary>
    Inserted = 1,

    /// <summary>
    ///  An existing track id was overwritten with the supplied hashes.
    /// </summary>
    Updated = 2,

    /// <summary>
    ///  The insert was skipped because the track is a full duplicate of an existing track
    ///  (prevent-duplicate-inserts). The duplicated track is named by <see cref="InsertResult.DuplicateOfTrackId"/>.
    /// </summary>
    DuplicateSuppressed = 3,

    // values 4+ are reserved for future outcomes; never reuse 0.
}