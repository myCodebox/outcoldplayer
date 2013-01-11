// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Net.Http.Headers;

    public static class MediaTypeHeaderValueExtensions
    {
        public static bool IsPlainText(this MediaTypeHeaderValue @this)
        {
            if (@this == null)
            {
                return false;
            }

            return string.Equals(@this.MediaType, "text/plain", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsHtmlText(this MediaTypeHeaderValue @this)
        {
            if (@this == null)
            {
                return false;
            }

            return string.Equals(@this.MediaType, "text/html", StringComparison.OrdinalIgnoreCase);
        }
    }
}