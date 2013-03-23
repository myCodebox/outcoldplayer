// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;

    using OutcoldSolutions.GoogleMusic.Services;

    public static class SettingsServiceEx
    {
        private const string LibraryFreshnessDateKey = "LibraryFreshnessDate";

        public static DateTime? GetLibraryFreshnessDate(this ISettingsService @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            return @this.GetValue<DateTime?>(LibraryFreshnessDateKey);
        }

        public static void SetLibraryFreshnessDate(this ISettingsService @this, DateTime dateTime)
        {
            @this.SetValue(LibraryFreshnessDateKey, dateTime);
        }

        public static void ResetLibraryFreshness(this ISettingsService @this)
        {
            @this.RemoveValue(LibraryFreshnessDateKey);
        }
    }
}
