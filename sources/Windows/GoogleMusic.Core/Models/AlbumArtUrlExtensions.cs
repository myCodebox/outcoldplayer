// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;
    using System.Globalization;

    public static class AlbumArtUrlExtensions
    {
        private const string AlbumArtUrlParameter = "=s130-c-e100";

        public static Uri ChangeSize(this Uri uri, uint size)
        {
            if (uri == null)
            {
                return null;
            }

            string url = uri.ToString();

            if (url.LastIndexOf(AlbumArtUrlParameter, StringComparison.OrdinalIgnoreCase) == (url.Length - AlbumArtUrlParameter.Length))
            {
                url = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}=s{1}-c-e100",
                    url.Substring(0, url.Length - AlbumArtUrlParameter.Length),
                    size);
            }

            return new Uri(url);
        }

        public static Uri ToLocalUri(string path)
        {
            return new Uri(string.Format(CultureInfo.InvariantCulture, "ms-appdata:///local/{0}", path));
        }
    }
}
