namespace SoundFingerprinting.Content;

using System;

/// <summary>
///  A class holding all the information required for a streaming query.
/// </summary>
public class StreamingFile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamingFile"/> class.
    /// </summary>
    /// <param name="path">Path to media file.</param>
    /// <param name="relativeTo">Timestamp relative to.</param>
    /// <param name="streamId">Stream identifier.</param>
    public StreamingFile(string path, DateTime relativeTo, string streamId)
    {
        StreamId = streamId;
        RelativeTo = relativeTo;
        Path = path;
    }

    /// <summary>
    ///  Gets path to media file.
    /// </summary>
    public string Path { get; }
    
    /// <summary>
    ///  Gets stream identifier.
    /// </summary>
    public string StreamId { get; }

    /// <summary>
    ///  Gets relative to timestamp.
    /// </summary>
    public DateTime RelativeTo { get; }

    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
    {
        return $"StreamId={StreamId},Date={RelativeTo:O},Path={Path}";
    }
}