namespace Soundfingerprinting.Hashing.Utils
{
    /// <summary>
    ///   Support Interface for Precision Operations (like AlmostEquals).
    /// </summary>
    /// <typeparam name = "T">Type of the implementing class.</typeparam>
    public interface IPrecisionSupport<T>
    {
        /// <summary>
        ///   Returns a Norm of a value of this type, which is appropriate for measuring how
        ///   close this value is to zero.
        /// </summary>
        /// <returns>A norm of this value.</returns>
        double Norm();

        /// <summary>
        ///   Returns a Norm of the difference of two values of this type, which is
        ///   appropriate for measuring how close together these two values are.
        /// </summary>
        /// <param name = "otherValue">The value to compare with.</param>
        /// <returns>A norm of the difference between this and the other value.</returns>
        double NormOfDifference(T otherValue);
    }
}