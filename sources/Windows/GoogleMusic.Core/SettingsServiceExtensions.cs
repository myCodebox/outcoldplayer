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
        private const string StreamBitrateKey = "StreamBitrate";

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
    }
}
