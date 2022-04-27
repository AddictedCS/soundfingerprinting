namespace SoundFingerprinting.Content;

using System;
using SoundFingerprinting.Data;

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
    /// <param name="mediaType">Media type to fingerprint.</param>
    public StreamingFile(string path, DateTime relativeTo, string streamId, MediaType mediaType)
    {
        StreamId = streamId;
        MediaType = mediaType;
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
    ///  Gets media type to fingerprint from the file.
    /// </summary>
    public MediaType MediaType { get; }

    /// <summary>
    ///  Gets relative to timestamp.
    /// </summary>
    public DateTime RelativeTo { get; }

    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
    {
        return $"Path={Path},StreamId={StreamId},Date={RelativeTo:O},MediaType={MediaType}";
    }
}