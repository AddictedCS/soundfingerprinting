namespace SoundFingerprinting.DAO
{
    using System.Threading;
    using ProtoBuf;
    
    [ProtoContract]
    public class UIntModelReferenceProvider : IModelReferenceProvider
    {
        [ProtoMember(1)]
        private long referenceCounter;
        
        [ProtoMember(2)]
        private long maxAllowedReference = int.MaxValue;

        public UIntModelReferenceProvider(long referenceCounter = 0, long maxAllowedReference = int.MaxValue)
        {
            if (referenceCounter > maxAllowedReference)
            {
                throw new ModelReferenceMaxAllowedValueExceededException(referenceCounter);
            }
            
            this.referenceCounter = referenceCounter;
            this.maxAllowedReference = maxAllowedReference;
        }

        private UIntModelReferenceProvider()
        {
            // left for protobuf
        }
        
        public long Current => Interlocked.Read(ref referenceCounter);

        public IModelReference Next()
        {
            long increment = Interlocked.Increment(ref referenceCounter);
            if (increment > maxAllowedReference)
            {
                throw new ModelReferenceMaxAllowedValueExceededException(increment);
            }
            
            var next = (uint)increment;
            return new ModelReference<uint>(next);
        }

        /// <summary>
        ///  Issues the next reference, or reports that the counter reached <c>maxAllowedReference</c>.
        /// </summary>
        /// <param name="reference">The issued reference, or null when the counter reached the maximum.</param>
        /// <returns>True when this call issues a reference, false when the next value exceeds the maximum.</returns>
        /// <remarks>
        ///  <see cref="Next"/> increments first and throws after, which leaves the counter above the maximum and
        ///  forces the caller to discard the provider. This method leaves the counter untouched when it cannot
        ///  issue, so a caller that recovers from exhaustion (a rolling window that re-issues low references)
        ///  needs neither an exception nor a replacement provider.
        /// </remarks>
        public bool TryNext(out IModelReference? reference)
        {
            while (true)
            {
                long current = Interlocked.Read(ref referenceCounter);
                long next = current + 1;
                if (next > maxAllowedReference)
                {
                    reference = null;
                    return false;
                }

                if (Interlocked.CompareExchange(ref referenceCounter, next, current) == current)
                {
                    reference = new ModelReference<uint>((uint)next);
                    return true;
                }
            }
        }
    }
}
