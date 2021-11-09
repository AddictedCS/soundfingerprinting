namespace SoundFingerprinting.Query
{
    using System;

    /// <summary>
    ///  Flags describing the query match.
    /// </summary>
    [Flags]
    public enum ReviewStatus
    {
        /// <summary>
        ///  No review is required.
        /// </summary>
        None = 0,
        
        /// <summary>
        ///  <see cref="AVResultEntry"/> needs a review because of possible issues with the match
        /// </summary>
        /// <remarks>
        ///   Currently Video overlays are detected automatically, getting marked as requiring review.
        /// </remarks>
        RequiresReview = 1,
        
        /// <summary>
        ///  Reviewed and found the exact match.
        /// </summary>
        ReviewedSuccess = 2,
        
        /// <summary>
        ///  Reviewed and found the exact match.
        /// </summary>
        ReviewedFailure = 3,
        
        /// <summary>
        ///  The system reviewed the match in auto mode and successfully found the exact match.
        /// </summary>
        /// <remarks>
        ///  Automatic review is performed according to the registered Rules.
        /// </remarks>
        ReviewedSuccessAuto = 4,
        
        /// <summary>
        ///  The system reviewed the match in auto mode and failed to find the exact match. 
        /// </summary>
        /// <remarks>
        ///  Automatic review is performed according to the registered Rules.
        /// </remarks>
        ReviewedFailureAuto = 5
    }
}