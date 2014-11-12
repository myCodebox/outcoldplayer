// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Globalization;

    /// <summary>
    /// The package version extension methods.
    /// </summary>
    public static class VersionExtensions
    {
        /// <summary>
        /// Convert <see cref="Version"/> into version string.
        /// </summary>
        /// <param name="packageVersion">
        /// The package version.
        /// </param>
        /// <returns>
        /// The version <see cref="string"/>.
        /// </returns>
        public static string ToVersionString(this Version packageVersion)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "v{0}.{1}.{2}.{3}",
                packageVersion.Major,
                packageVersion.Minor,
                packageVersion.Build,
                packageVersion.Revision);
        }
    }
}