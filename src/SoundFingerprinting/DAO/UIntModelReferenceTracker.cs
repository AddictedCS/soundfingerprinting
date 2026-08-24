namespace SoundFingerprinting.DAO
{
    using System.Collections.Generic;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    /// <inheritdoc />
    /// <remarks>
    ///  With <paramref name="wrapOnOverflow"/> set, exhaustion is not an error: both counters restart at 0 together
    ///  and the assignment repeats, so the whole track lands in one contiguous low block and never straddles the
    ///  wrap. A rolling window (strict FIFO, oldest references deleted long before the counter laps) is the only
    ///  correct user of that mode. A persistent corpus must leave it off and recover from exhaustion by
    ///  defragmentation, because a wrap re-issues references that the corpus still holds.
    /// </remarks>
    /// <param name="trackRef">First track reference to issue from.</param>
    /// <param name="subFingerprintRef">First sub-fingerprint reference to issue from.</param>
    /// <param name="maxAllowedReference">Largest reference that the tracker issues.</param>
    /// <param name="wrapOnOverflow">Set to restart both counters at 0 instead of throwing on exhaustion.</param>
    public class UIntModelReferenceTracker(uint trackRef = 0, uint subFingerprintRef = 0, long maxAllowedReference = int.MaxValue, bool wrapOnOverflow = false) : IModelReferenceTracker<uint>
    {
        private readonly object @object = new ();
        
        private UIntModelReferenceProvider trackReferenceProvider = new (trackRef, maxAllowedReference);
        private UIntModelReferenceProvider subFingerprintsReferenceProvider = new (subFingerprintRef, maxAllowedReference);
        private long lap;

        /// <summary>
        ///  Gets the lap count and both write heads as one atomic snapshot.
        /// </summary>
        public UIntModelReferenceState State
        {
            get
            {
                lock (@object)
                {
                    return new UIntModelReferenceState(lap, trackReferenceProvider.Current, subFingerprintsReferenceProvider.Current);
                }
            }
        }

        /// <inheritdoc />
        public bool TryResetTrackRef(uint maxTrackRef)
        {
            lock (@object)
            {
                if (maxTrackRef > trackReferenceProvider.Current)
                {
                    trackReferenceProvider = new UIntModelReferenceProvider(maxTrackRef, maxAllowedReference);
                    return true;
                }

                return false;
            }
        }

        /// <inheritdoc />
        public bool TryResetSubFingerprintRef(uint maxSubFingerprintRef)
        {
            lock (@object)
            {
                if (maxSubFingerprintRef > subFingerprintsReferenceProvider.Current)
                {
                    subFingerprintsReferenceProvider = new UIntModelReferenceProvider(maxSubFingerprintRef, maxAllowedReference);
                    return true;
                }

                return false;
            }
        }

        /// <inheritdoc />
        public LinkedDataModels AssignModelReferences(TrackInfo trackInfo, Hashes hashes)
        {
            lock (@object)
            {
                var models = TryAssign(trackInfo, hashes);
                if (models != null)
                {
                    return models;
                }

                if (!wrapOnOverflow)
                {
                    throw new ModelReferenceMaxAllowedValueExceededException(maxAllowedReference + 1);
                }

                // both counters restart together: a track that took its reference before the wrap and its
                // sub-fingerprints after it would straddle the boundary and break their co-monotonic order
                trackReferenceProvider = new UIntModelReferenceProvider(0, maxAllowedReference);
                subFingerprintsReferenceProvider = new UIntModelReferenceProvider(0, maxAllowedReference);
                ++lap;

                models = TryAssign(trackInfo, hashes);
                if (models != null)
                {
                    return models;
                }

                // one track asks for more references than a whole lap holds, so no wrap count makes it fit
                throw new ModelReferenceMaxAllowedValueExceededException((long)hashes.Count + 1);
            }
        }

        private LinkedDataModels? TryAssign(TrackInfo trackInfo, Hashes hashes)
        {
            if (!trackReferenceProvider.TryNext(out var trackReference) || trackReference == null)
            {
                return null;
            }

            var trackData = new TrackData(trackInfo.Id, trackInfo.Artist, trackInfo.Title, hashes.DurationInSeconds, trackReference, trackInfo.MetaFields, trackInfo.MediaType);
            var subFingerprints = new List<SubFingerprintData>(hashes.Count);
            foreach (var hash in hashes)
            {
                if (!subFingerprintsReferenceProvider.TryNext(out var subFingerprintReference) || subFingerprintReference == null)
                {
                    return null;
                }

                subFingerprints.Add(new SubFingerprintData(hash.HashBins, hash.SequenceNumber, hash.StartsAt, subFingerprintReference, trackReference, hash.OriginalPoint));
            }

            return new LinkedDataModels(trackData, subFingerprints);
        }
    }
}
