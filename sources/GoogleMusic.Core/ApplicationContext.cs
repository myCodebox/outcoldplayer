// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using OutcoldSolutions.GoogleMusic.InversionOfControl;

    public static class ApplicationContext
    {
        /// <summary>
        /// Gets the container.
        /// </summary>
        public static IDependencyResolverContainer Container { get; set; }

        /// <summary>
        /// Gets application local folder.
        /// </summary>
        public static IFolder ApplicationLocalFolder { get; set; }

        /// <summary>
        /// Get application version.
        /// </summary>
        public static Version ApplicationVersion { get; set; }
    }
}
