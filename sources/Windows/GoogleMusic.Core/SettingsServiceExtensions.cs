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
        public const string LibraryFreshnessDateKey = "LibraryFreshnessDate";
        public const string StreamBitrateKey = "StreamBitrate";
        public const string IsMusicLibraryForCacheKey = "IsMusicLibraryForCache";
        public const string IsThumbsRatingKey = "ThumbsRating";

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

        public static bool GetIsMusicLibraryForCache(this ISettingsService @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            return @this.GetValue<bool>(IsMusicLibraryForCacheKey, defaultValue: false);
        }

        public static void SetIsMusicLibraryForCache(this ISettingsService @this, bool value)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            @this.SetValue(IsMusicLibraryForCacheKey, value);
        }

        public static bool GetIsThumbsRating(this ISettingsService @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            return @this.GetValue<bool>(IsThumbsRatingKey, defaultValue: false);
        }

        public static void SetIsThumbsRating(this ISettingsService @this, bool value)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            @this.SetValue(IsThumbsRatingKey, value);
        }
    }
}
