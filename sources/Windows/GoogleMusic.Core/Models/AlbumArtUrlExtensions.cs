// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;
    using System.Globalization;

    public static class AlbumArtUrlExtensions
    {
        public const uint DefaultAlbumArtSize = 130;

        private const string AlbumArtUrlParameter = "=s512";

        public static Uri ChangeSize(this Uri uri, uint size)
        {
            if (uri == null)
            {
                return null;
            }

            string url = uri.ToString();

            var lastIndexOf = url.LastIndexOf(AlbumArtUrlParameter, StringComparison.OrdinalIgnoreCase);
            if (lastIndexOf == (url.Length - AlbumArtUrlParameter.Length))
            {
                url = url.Substring(0, url.Length - AlbumArtUrlParameter.Length);
            }

            url = string.Format(CultureInfo.InvariantCulture, "{0}=s{1}", url, size);

            return new Uri(url);
        }

        public static Uri ToLocalUri(string path)
        {
            return new Uri(string.Format(CultureInfo.InvariantCulture, "ms-appdata:///local/{0}", path));
        }
    }
}
