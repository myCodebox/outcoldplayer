// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Globalization;

    using Windows.ApplicationModel;

    public static class PackageVersionExtensions
    {
        public static string ToVersionString(this PackageVersion packageVersion)
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