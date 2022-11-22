namespace SoundFingerprinting.DAO
{
    using System;
    using ProtoBuf;

    /// <summary>
    ///  Storage model reference.
    /// </summary>
    /// <typeparam name="T">Type parameter.</typeparam>
    [Serializable]
    [ProtoContract]
    public class ModelReference<T> : IModelReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelReference{T}"/> class.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public ModelReference(T id)
        {
            Id = id;
        }

        private ModelReference()
        {
            // left for proto-buf
        }

        /// <summary>
        ///  Gets a null model reference.
        /// </summary>
        public static ModelReference<T> Null { get; } = new ModelReference<T>(default);

        /// <summary>
        ///  Gets underlying identifier for the model reference.
        /// </summary>
        [ProtoMember(1)]
        public T Id { get; }
        
        /// <summary>
        ///  Casts underlying <see cref="Id"/> to requested type.
        /// </summary>
        /// <typeparam name="TOut">Type parameter.</typeparam>
        /// <returns><see cref="Id"/> casted to request type parameter.</returns>
        /// <exception cref="InvalidCastException">Throws invalid cast exception.</exception>
        public TOut Get<TOut>()
        {
            if (Id is TOut get)
            {
                return get;
            }

            throw new InvalidCastException($"{typeof(T)} cannot be converted to {typeof(TOut)}");
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        public override bool Equals(object obj)
        {
            if (obj is not ModelReference<T> @object)
            {
                return false;
            }

            return Id.Equals(@object.Id); 
        }

        /// <inheritdoc cref="object.GetHashCode"/>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"ModelReference {nameof(Id)}: {Id}";
        }
    }
}
