namespace SoundFingerprinting.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using ProtoBuf;
    using SoundFingerprinting.Query;

    /// <summary>
    ///  Hashes class representing fingerprints.
    /// </summary>
    [Serializable]
    [ProtoContract(IgnoreListHandling = true, SkipConstructor = true)]
    public class Hashes : IEnumerable<HashedFingerprint>
    {
        private static IDictionary<string, string> emptyDictionary = new Dictionary<string, string>();

        [ProtoMember(1)]
        private readonly List<HashedFingerprint> fingerprints;

        [ProtoMember(7)]
        private readonly Dictionary<string, string>? additionalProperties;
        
        [ProtoIgnore]
        private List<HashedFingerprint> Fingerprints => fingerprints ?? Enumerable.Empty<HashedFingerprint>().ToList();

        /// <summary>
        ///  Initializes a new instance of the <see cref="Hashes"/> class.
        /// </summary>
        /// <param name="fingerprints">Fingerprints.</param>
        /// <param name="durationInSeconds">Duration in seconds of the media from which the fingerprints have been generated.</param>
        /// <param name="mediaType">Media type.</param>
        public Hashes(IEnumerable<HashedFingerprint> fingerprints, double durationInSeconds, MediaType mediaType) : this(fingerprints,
                durationInSeconds,
                mediaType,
                DateTime.UtcNow,
                Enumerable.Empty<string>(),
                string.Empty,
                emptyDictionary,
                0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hashes"/> class.
        /// </summary>
        /// <param name="fingerprints">Fingerprints.</param>
        /// <param name="durationInSeconds">Duration in seconds of the media from which the fingerprints have been generated.</param>
        /// <param name="mediaType">Media type.</param>
        /// <param name="relativeTo">Relative to a particular date time.</param> 
        public Hashes(IEnumerable<HashedFingerprint> fingerprints, double durationInSeconds, MediaType mediaType, DateTime relativeTo) : this(
            fingerprints,
            durationInSeconds,
            mediaType,
            relativeTo,
            Enumerable.Empty<string>(),
            string.Empty,
            emptyDictionary,
            0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hashes"/> class.
        /// </summary>
        /// <param name="fingerprints">Fingerprints.</param>
        /// <param name="durationInSeconds">Duration in seconds of the media from which the fingerprints have been generated.</param>
        /// <param name="mediaType">Media type.</param>
        /// <param name="relativeTo">Relative to a particular date time.</param>
        /// <param name="origins">List of strings referring to the origin of the hashes.</param>
        public Hashes(IEnumerable<HashedFingerprint> fingerprints, double durationInSeconds, MediaType mediaType, DateTime relativeTo,
            IEnumerable<string> origins) : this(fingerprints,
            durationInSeconds,
            mediaType,
            relativeTo,
            origins,
            string.Empty,
            emptyDictionary,
            0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hashes"/> class.
        /// </summary>
        /// <param name="fingerprints">Fingerprints.</param>
        /// <param name="durationInSeconds">Duration in seconds of the media from which the fingerprints have been generated.</param>
        /// <param name="mediaType">Media type.</param>
        /// <param name="relativeTo">Relative to a particular date time.</param>
        /// <param name="origins">List of strings referring to the origin of the hashes.</param>
        /// <param name="streamId">Stream from which the hashes originate.</param>
        public Hashes(IEnumerable<HashedFingerprint> fingerprints, double durationInSeconds, MediaType mediaType, DateTime relativeTo,
            IEnumerable<string> origins, string streamId) : this(fingerprints,
                durationInSeconds,
                mediaType,
                relativeTo,
                origins,
                streamId,
                emptyDictionary,
                0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hashes"/> class.
        /// </summary>
        /// <param name="fingerprints">Fingerprints.</param>
        /// <param name="durationInSeconds">Duration in seconds of the media from which the fingerprints have been generated.</param>
        /// <param name="mediaType">Media type.</param>
        /// <param name="relativeTo">Relative to a particular date time.</param>
        /// <param name="origins">List of strings referring to the origin of the hashes.</param>
        /// <param name="streamId">Stream from which the hashes originate.</param>
        /// <param name="additionalProperties">Dictionary with additional properties.</param>
        /// <param name="timeOffset">Time offset in seconds.</param>
        public Hashes(IEnumerable<HashedFingerprint> fingerprints,
            double durationInSeconds,
            MediaType mediaType,
            DateTime relativeTo,
            IEnumerable<string> origins,
            string streamId,
            IDictionary<string, string> additionalProperties,
            double timeOffset)
        {
            this.fingerprints = fingerprints.ToList();
            if (this.fingerprints.Any() && durationInSeconds <= 0)
            {
                throw new ArgumentException(nameof(durationInSeconds));
            }

            this.additionalProperties = new Dictionary<string, string>(additionalProperties);
            
            DurationInSeconds = durationInSeconds;
            RelativeTo = relativeTo;
            Origins = origins;
            StreamId = streamId;
            MediaType = mediaType;
            TimeOffset = timeOffset;
        }

        /// <summary>
        ///  Gets hashes duration in seconds, equal to the length of the original media file.
        /// </summary>
        [ProtoMember(2)]
        public double DurationInSeconds { get; }

        /// <summary>
        ///  Gets the stream ID associated with this instance.
        /// </summary>
        [ProtoMember(3)] 
        public string StreamId { get; }

        /// <summary>
        ///  Gets an actual point in time reference of when these hashes have been generated.
        /// </summary>
        [ProtoMember(4)]
        public DateTime RelativeTo { get; }

        /// <summary>
        ///  Gets a list of origins of the hashes.
        /// </summary>
        [ProtoMember(5)]
        public IEnumerable<string> Origins { get; }

        /// <summary>
        ///  Gets the media type associated with this hashes object.
        /// </summary>
        [ProtoMember(6)]
        public MediaType MediaType { get; }

        /// <summary>
        ///  Gets additional properties associated with provided hashes object.
        /// </summary>
        public IReadOnlyDictionary<string, string> Properties => additionalProperties ?? new Dictionary<string, string>();

        /// <summary>
        ///  Gets time offset used during query time, <see cref="ResultEntryConcatenator.Concat"/>.
        /// </summary>
        /// <remarks>
        ///  It is used only for realtime query hashes.
        /// </remarks>
        [ProtoMember(8)]
        public double TimeOffset { get; }

        /// <summary>
        ///  Gets an actual time reference when these hashes end.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///  If hashes are empty of <see cref="RelativeTo"/> property is not set.
        /// </exception>
        public DateTime EndsAt
        {
            get
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Hashes are empty, EndsAt is undefined");
                }

                if (RelativeTo.Equals(DateTime.MinValue))
                {
                    throw new InvalidOperationException("RelativeTo was not supplied as a parameter to find EndsAt");
                }
                
                return RelativeTo.Add(TimeSpan.FromSeconds(DurationInSeconds));
            }
        }

        /// <summary>
        ///  Gets a value indicating whether hashes object is empty.
        /// </summary>
        public bool IsEmpty => !Fingerprints.Any();
        
        /// <summary>
        ///  Gets number of contained fingerprints.
        /// </summary>
        public int Count => IsEmpty ? 0 : Fingerprints.Count;

        /// <summary>
        ///  Hashes indexer.
        /// </summary>
        /// <param name="index">Get element from index.</param>
        public HashedFingerprint this[int index] => fingerprints[index];

        /// <summary>
        ///  Creates a new empty hashes object.
        /// </summary>
        /// <param name="mediaType">Media type to associated empty hashes with.</param>
        /// <returns>Empty instance of <see cref="Hashes"/> class.</returns>
        public static Hashes GetEmpty(MediaType mediaType) => new Hashes(new List<HashedFingerprint>(), 0, mediaType, DateTime.MinValue, new List<string>(), string.Empty, emptyDictionary, 0);
        
        /// <summary>
        ///  Add a stream identifier to hashes object.
        /// </summary>
        /// <param name="streamId">Stream ID.</param>
        /// <returns>New instance of <see cref="Hashes"/> class.</returns>
        public Hashes WithStreamId(string streamId)
        {
            return new Hashes(Fingerprints, DurationInSeconds, MediaType, RelativeTo, Origins, streamId, additionalProperties ?? emptyDictionary, TimeOffset);
        }

        /// <summary>
        ///  Overrides current <see cref="RelativeTo"/> with a new one.
        /// </summary>
        /// <param name="relativeTo">New relative to property.</param>
        /// <returns>New instance of <see cref="Hashes"/> object with newly set <see cref="RelativeTo"/> property.</returns>
        public Hashes WithRelativeTo(DateTime relativeTo)
        {
            return new Hashes(Fingerprints, DurationInSeconds, MediaType, relativeTo, Origins, StreamId, additionalProperties ?? emptyDictionary, 0);
        }

        /// <summary>
        ///  Adds new additional property to hashes object.
        /// </summary>
        /// <param name="key">Property key.</param>
        /// <param name="value">Property value.</param>
        /// <returns>New instance of <see cref="Hashes"/> object with new additional properties object.</returns>
        public Hashes WithProperty(string key, string value)
        {
            if (additionalProperties == null)
            {
                return new Hashes(Fingerprints, DurationInSeconds, MediaType, RelativeTo, Origins, StreamId, new Dictionary<string, string> {{key, value}}, TimeOffset);
            }
            
            additionalProperties[key] = value;
            return this;
        }

        /// <summary>
        ///  Sets time offset in seconds on <see cref="Hashes"/> object.
        /// </summary>
        /// <param name="timeOffset">Time offset in seconds to set on current object.</param>
        /// <returns>New hashes with provided time offset.</returns>
        public Hashes WithTimeOffset(double timeOffset)
        {
            return new Hashes(Fingerprints, DurationInSeconds, MediaType, RelativeTo, Origins, StreamId, additionalProperties ?? emptyDictionary, timeOffset);
        }

        /// <summary>
        ///  Gets enumerator over actual fingerprints.
        /// </summary>
        /// <returns>Instance of the <see cref="IEnumerable{HashedFingerprint}"/> interface.</returns>
        public IEnumerator<HashedFingerprint> GetEnumerator()
        {
            return Fingerprints.GetEnumerator();
        }

        /// <inheritdoc cref="GetEnumerator"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///  Gets new range from the current hashes object.
        /// </summary>
        /// <param name="startsAt">Start at (as relative to <see cref="RelativeTo"/>.</param>
        /// <param name="length">Length in seconds of the new <see cref="Hashes"/> object.</param>
        /// <returns>New hashes object.</returns>
        public Hashes GetRange(DateTime startsAt, float length)
        {
            if (IsEmpty)
            {
                return GetEmpty(MediaType);
            }
            
            var endsAt = startsAt.AddSeconds(length);
            var ordered = fingerprints.OrderBy(_ => _.SequenceNumber).ToList();
            var lengthOfOneFingerprint = DurationInSeconds - ordered.Last().StartsAt;
            
            var filtered = ordered.Where(fingerprint =>
            {
                var fingerprintStartsAt = RelativeTo.AddSeconds(fingerprint.StartsAt);
                var fingerprintEndsAt = RelativeTo.AddSeconds(fingerprint.StartsAt + lengthOfOneFingerprint);
                return fingerprintStartsAt >= startsAt && fingerprintEndsAt <= endsAt;
            })
            .ToList();
            
            if (!filtered.Any())
            {
                return new Hashes(Enumerable.Empty<HashedFingerprint>(), 0, MediaType, startsAt);
            }

            var relativeTo = RelativeTo.AddSeconds(filtered.First().StartsAt);
            var duration = filtered.Last().StartsAt - filtered.First().StartsAt + lengthOfOneFingerprint;
            var shifted = ShiftStartsAtAccordingToSelectedRange(filtered);
            return new Hashes(shifted, duration, MediaType, relativeTo, Origins, StreamId, additionalProperties ?? emptyDictionary, 0);
        }

        private static List<HashedFingerprint> ShiftStartsAtAccordingToSelectedRange(List<HashedFingerprint> filtered)
        {
            var startsAtShift = filtered.First().StartsAt;
            return filtered.Select((fingerprint,
                    index) => new HashedFingerprint(fingerprint.HashBins,
                    (uint)index,
                    fingerprint.StartsAt - startsAtShift,
                    fingerprint.OriginalPoint))
                .ToList();
        }

        /// <summary>
        ///  Merge current object with provided hashes.
        /// </summary>
        /// <param name="with">Hashes to merge with.</param>
        /// <exception cref="ArgumentException">
        ///  Thrown when <see cref="MediaType"/> between merged hashes is not the same, or stream IDs are not the same.
        /// </exception>
        /// <returns>Merged instance of <see cref="Hashes"/> class.</returns>
        public Hashes MergeWith(Hashes? with)
        {
            if (with == null)
            {
                return this;
            }
            
            if (MediaType != with.MediaType)
            {
                throw new ArgumentException($"Can't merge hashes with different media types {MediaType}!={with.MediaType}", nameof(with.MediaType));
            }
            
            string streamId = (StreamId, with.StreamId) switch
            {
                ("", "") => "",
                (string left, "") => left,
                ("", string right) => right,
                (string left, string right) when left.Equals(right) => left,
                _ => throw new ArgumentException($"Can't merge two hash sequences that come with different streams {StreamId}, {with.StreamId}")
            };

            if (IsEmpty)
            {
                return with;
            }

            if (with.IsEmpty)
            {
                return this;
            }

            return Merge(this, with, streamId);
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"Hashes[Count:{Count},Length:{DurationInSeconds:0.00},RelativeTo={RelativeTo:O},StreamId={StreamId},MediaType={MediaType}]";
        }
        
        /// <summary>
        ///  Aggregates list of <see cref="Hashes"/> into chunks of given length.
        /// </summary>
        /// <param name="hashes">List of hashes with chunks of given length.</param>
        /// <param name="length">Required length of resulting hashes.</param>
        /// <returns>List of <see cref="Hashes"/> object.</returns>
        /// <exception cref="ArgumentException">All hashes must have the same media type and stream id.</exception>
        public static IEnumerable<Hashes> Aggregate(IEnumerable<Hashes> hashes, double length)
        {
            var list = hashes.ToList();

            if (!list.Any())
            {
                return Enumerable.Empty<Hashes>();
            }
            
            for (int i = 1; i < list.Count; ++i)
            {
                if (list[i].MediaType != list[i - 1].MediaType)
                {
                    throw new ArgumentException($"Cannot aggregate list of hashes with different media types", nameof(hashes));
                }
            }

            var mediaType = list[0].MediaType;
            return list
                .OrderBy(entry => entry.RelativeTo)
                .Aggregate(new Stack<Hashes>(new[] {GetEmpty(mediaType)}), (stack, next) =>
                    {
                        var completed = stack.Pop();
                        var merged = completed.MergeWith(next);
                        stack.Push(merged);
                        if (merged.DurationInSeconds >= length)
                        {
                            stack.Push(GetEmpty(mediaType));
                        }

                        return stack;
                    },
                    stack =>
                    {
                        var result = new List<Hashes>();
                        while (stack.Any())
                        {
                            var aggregated = stack.Pop();
                            if (!aggregated.IsEmpty)
                            {
                                result.Add(aggregated);
                            }
                        }

                        return result;
                    });
        }

        private static Hashes Merge(Hashes left, Hashes right, string streamId)
        {
            var first = left.OrderBy(_ => _.SequenceNumber).ToList();
            double leftTail = left.DurationInSeconds - first.Last().StartsAt;
            var firstStartsAt = left.RelativeTo;
            var second = right.OrderBy(_ => _.SequenceNumber).ToList();
            double rightTail = right.DurationInSeconds - second.Last().StartsAt;
            var secondStartsAt = right.RelativeTo;
                
            var result = new List<HashedFingerprint>();
            int i = 0, j = 0;
            var diff = secondStartsAt.Subtract(firstStartsAt);
            double tailLength = 0;
            for (int k = 0; k < first.Count + second.Count; ++k)
            {
                if (i == first.Count)
                {
                    var startAt = diff.TotalSeconds + second[j].StartsAt;
                    result.Add(new HashedFingerprint(second[j].HashBins, (uint)k, (float)startAt, second[j].OriginalPoint));
                    ++j;
                    tailLength = rightTail;
                }
                else if (j == second.Count)
                {
                    result.Add(new HashedFingerprint(first[i].HashBins, (uint)k, first[i].StartsAt, first[i].OriginalPoint));
                    ++i;
                    tailLength = leftTail;
                }
                else if (firstStartsAt.AddSeconds(first[i].StartsAt) <= secondStartsAt.AddSeconds(second[j].StartsAt))
                {
                    result.Add(new HashedFingerprint(first[i].HashBins, (uint)k, first[i].StartsAt, first[i].OriginalPoint));
                    ++i;
                }
                else
                {
                    var startAt = diff.TotalSeconds + second[j].StartsAt;
                    result.Add(new HashedFingerprint(second[j].HashBins, (uint)k, (float)startAt, second[j].OriginalPoint));
                    ++j;
                }
            }
            
            var relativeTo = left.RelativeTo < right.RelativeTo ? left.RelativeTo : right.RelativeTo;
            var fullLength = result.Last().StartsAt + tailLength;

            var additionalProperties = new Dictionary<string, string>();
            foreach (var kv in left.Properties.Concat(right.Properties))
            {
                additionalProperties[kv.Key] = kv.Value;
            }

            double offset = left.RelativeTo < right.RelativeTo ? left.TimeOffset : right.TimeOffset;
            return new Hashes(result, fullLength, left.MediaType, relativeTo, new HashSet<string>(left.Origins.Concat(right.Origins)), streamId, additionalProperties, offset);
        }
    }
}