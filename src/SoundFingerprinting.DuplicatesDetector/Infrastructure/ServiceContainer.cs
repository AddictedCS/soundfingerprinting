namespace SoundFingerprinting.DuplicatesDetector.Infrastructure
{
    using Ninject;

    /// <summary>
    ///   Class which will hold services injected by dependency injection on Application startup
    ///   Ninject lib is used for cross class injection
    /// </summary>
    /// <remarks>
    ///   Follows the Service Locator pattern.
    ///   More details can be found here:
    ///   http://martinfowler.com/articles/injection.html
    /// </remarks>
    public static class ServiceContainer
    {
        /// <summary>
        ///   Actual service container
        /// </summary>
        public static readonly IKernel Kernel = new StandardKernel();
    }
}