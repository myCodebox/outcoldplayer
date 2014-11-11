// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using Windows.UI.Xaml;

    using OutcoldSolutions.GoogleMusic.Diagnostics;

    /// <summary>
    /// The application base.
    /// </summary>
    public abstract class ApplicationBase : Application
    {
        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger { get; set; }
    }
}