namespace SoundFingerprinting.Hashing.NeuralHashing.Utils
{
    using System;

    /// <summary>
    ///   Represents a double range with minimum and maximum values.
    /// </summary>
    /// <remarks>
    ///   <para>The class represents a double range with inclusive limits -
    ///     both minimum and maximum values of the range are included into it.
    ///     Mathematical notation of such range is <b>[min, max]</b>.
    ///   </para>
    /// </remarks>
    [Serializable]
    public class FloatRange
    {
        private float max;
        private float min;

        /// <summary>
        ///   Initializes a new instance of the <see cref = "FloatRange" /> class.
        /// </summary>
        /// <param name = "min">Minimum value of the range.</param>
        /// <param name = "max">Maximum value of the range.</param>
        public FloatRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        /// <summary>
        ///   Minimum value of the range.
        /// </summary>
        /// <remarks>
        ///   <para>The property represents minimum value (left side limit) or the range -
        ///     [<b>min</b>, max].</para>
        /// </remarks>
        public float Min
        {
            get { return min; }
            set { min = value; }
        }

        /// <summary>
        ///   Maximum value of the range.
        /// </summary>
        /// <remarks>
        ///   <para>The property represents maximum value (right side limit) or the range -
        ///     [min, <b>max</b>].</para>
        /// </remarks>
        public float Max
        {
            get { return max; }
            set { max = value; }
        }

        /// <summary>
        ///   Length of the range (deffirence between maximum and minimum values).
        /// </summary>
        public float Length
        {
            get { return max - min; }
        }


        /// <summary>
        ///   Check if the specified value is inside of the range.
        /// </summary>
        /// <param name = "x">Value to check.</param>
        /// <returns><b>True</b> if the specified value is inside of the range or
        ///   <b>false</b> otherwise.</returns>
        public bool IsInside(float x)
        {
            return ((x >= min) && (x <= max));
        }

        /// <summary>
        ///   Check if the specified range is inside of the range.
        /// </summary>
        /// <param name = "range">Range to check.</param>
        /// <returns><b>True</b> if the specified range is inside of the range or
        ///   <b>false</b> otherwise.</returns>
        public bool IsInside(FloatRange range)
        {
            return ((IsInside(range.min)) && (IsInside(range.max)));
        }

        /// <summary>
        ///   Check if the specified range overlaps with the range.
        /// </summary>
        /// <param name = "range">Range to check for overlapping.</param>
        /// <returns><b>True</b> if the specified range overlaps with the range or
        ///   <b>false</b> otherwise.</returns>
        public bool IsOverlapping(FloatRange range)
        {
            return ((IsInside(range.min)) || (IsInside(range.max)));
        }
    }
}