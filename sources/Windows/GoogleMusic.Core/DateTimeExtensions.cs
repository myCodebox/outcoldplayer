// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;

    public static class DateTimeExtensions
    {
        private static readonly DateTime UnixBaseTime = new DateTime(1970, 1, 1, 0, 0, 0);

        public static DateTime FromUnixFileTime(double fileTime)
        {
            return UnixBaseTime.AddMilliseconds(fileTime);
        }
    }
}