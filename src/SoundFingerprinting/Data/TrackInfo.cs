namespace SoundFingerprinting.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ProtoBuf;

    /// <summary>
    ///  Class describing a track.
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class TrackInfo : IEquatable<TrackInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackInfo"/> class.
        /// </summary>
        /// <param name="id">Track unique identifier.</param>
        /// <param name="title">Track title.</param>
        /// <param name="artist">Track artist.</param>
        public TrackInfo(string id, string title, string artist) : this(id, title, artist, new Dictionary<string, string>())
        {
            // no op
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackInfo"/> class.
        /// </summary>
        /// <param name="id">Track unique identifier.</param>
        /// <param name="title">Track title.</param>
        /// <param name="artist">Track artist.</param>
        /// <param name="metaFields">Track meta fields.</param>
        /// <param name="mediaType">Track media type (default MediaType.Audio).</param>
        public TrackInfo(string id, string title, string artist, IDictionary<string, string> metaFields, MediaType mediaType = MediaType.Audio)
        {
            Id = id;
            Title = title;
            Artist = artist;
            MetaFields = metaFields;
            MediaType = mediaType;
        }

        private TrackInfo()
        {
            // left for proto-buf
            MetaFields = new Dictionary<string, string>();
            MediaType = MediaType.Audio;
        }

        /// <summary>
        ///  Gets tracks unique identifier.
        /// </summary>
        [ProtoMember(1)] 
        public string Id { get; }

        /// <summary>
        ///  Gets track title.
        /// </summary>
        [ProtoMember(2)] 
        public string Title { get; }

        /// <summary>
        ///  Gets track artist.
        /// </summary>
        [ProtoMember(3)] 
        public string Artist { get; }

        /// <summary>
        ///  Gets track meta fields.
        /// </summary>
        [ProtoMember(4)] 
        public IDictionary<string, string> MetaFields { get; }

        /// <summary>
        ///  Gets track media type.
        /// </summary>
        [ProtoMember(5)] 
        public MediaType MediaType { get; }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"TrackInfo Id: {Id}, " +
                   $"Title {Title}, " +
                   $"Artist {Artist}, " +
                   $"MediaType {MediaType}, " +
                   $"MetaFields {string.Join(",", MetaFields.Select(pair => $"{pair.Key}:{pair.Value}"))}";
        }

        /// <inheritdoc cref="object.GetHashCode"/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode() * 397) ^ (int)MediaType;
            }
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        public override bool Equals(object obj)
        {
            return Equals(obj as TrackInfo);
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        public bool Equals(TrackInfo? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id == other.Id && MediaType == other.MediaType;
        }
    }
}