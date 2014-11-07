// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Collections.Generic;
    using OutcoldSolutions.GoogleMusic.Services;

    public static class SettingsServiceExtensions
    {
        private const string LibraryFreshnessDateKey = "LibraryFreshnessDate";
        private const string AutomaticCacheKey = "AutomaticCache";
        private const string MaximumCacheSizeKey = "MaximumCacheSize";
        private const string StreamBitrateKey = "StreamBitrate";

        private const int MaximumOfflineSongsCount = 30;
        private const uint DefaultStreamBitrate = 320U;

        private static readonly IList<uint> Bitrates = new[] { 128U, 192U, 256U, DefaultStreamBitrate }; 

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

            if (InAppPurchases.HasFeature(GoogleMusicFeatures.Offline))
            {
                return @this.GetValue<uint>(MaximumCacheSizeKey, defaultValue: MaximumOfflineSongsCount);
            }

            return MaximumOfflineSongsCount;
        }

        public static void SetMaximumCacheSize(this ISettingsService @this, uint value)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            @this.SetValue(MaximumCacheSizeKey, value);
        }

        public static IList<uint> GetStreamBitrates(this ISettingsService @this)
        {
            return Bitrates;
        }

        public static uint GetStreamBitrate(this ISettingsService @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            uint val = @this.GetValue<uint>(StreamBitrateKey, defaultValue: DefaultStreamBitrate);
            if (!Bitrates.Contains(val))
            {
                return DefaultStreamBitrate;
            }
            return val;
        }

        public static void SetStreamBitrate(this ISettingsService @this, uint value)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            @this.SetValue(StreamBitrateKey, value);
        }
    }
}
