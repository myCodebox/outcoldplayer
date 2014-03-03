// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using Windows.UI.Xaml;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.InversionOfControl;

    /// <summary>
    /// The application base.
    /// </summary>
    public abstract class ApplicationBase : Application
    {
        /// <summary>
        /// Gets the container.
        /// </summary>
        public static IDependencyResolverContainer Container { get; protected set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger { get; set; }
    }
}