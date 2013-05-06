// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;

    using OutcoldSolutions.GoogleMusic.Services;

    public static class SettingsServiceExtensions
    {
        private const string LibraryFreshnessDateKey = "LibraryFreshnessDate";
        private const string AutomaticCacheKey = "AutomaticCache";
        private const string MaximumCacheSizeKey = "MaximumCacheSize";

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
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            @this.SetValue(LibraryFreshnessDateKey, dateTime);
        }

        public static void ResetLibraryFreshness(this ISettingsService @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            @this.RemoveValue(LibraryFreshnessDateKey);
        }

        public static bool GetAutomaticCache(this ISettingsService @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            return @this.GetValue<bool>(AutomaticCacheKey, defaultValue: false);
        }

        public static void SetAutomaticCache(this ISettingsService @this, bool value)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            @this.SetValue(AutomaticCacheKey, value);
        }

        public static uint GetMaximumCacheSize(this ISettingsService @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            return @this.GetValue<uint>(MaximumCacheSizeKey, defaultValue: 100);
        }

        public static void SetMaximumCacheSize(this ISettingsService @this, uint value)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            @this.SetValue(MaximumCacheSizeKey, value);
        }
    }
}
