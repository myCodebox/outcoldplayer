// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System;

    using OutcoldSolutions.GoogleMusic.Services;

    public static class GoogleMusicCoreSettingsServiceExtensions
    {
        public const string BlockExplicitSongsInRadioKey = "BlockExplicitSongsInRadio";

        public const string IsAllAccessAvailableKey = "IsAllAccessAvailable";

        public const string IsSearchLocalOnlyKey = "IsSearchLocalOnly";

        public const string LockScreenEnabledKey = "IsLockScreenEnabled";

        public static bool GetIsLockScreenEnabled(this ISettingsService @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            return @this.GetRoamingValue<bool>(LockScreenEnabledKey, defaultValue: true);
        }

        public static void SetIsLockScreenEnabled(this ISettingsService @this, bool value)
        {
            @this.SetRoamingValue(LockScreenEnabledKey, value);
        }

        public static bool GetBlockExplicitSongsInRadio(this ISettingsService @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            return @this.GetApplicationValue<bool>(BlockExplicitSongsInRadioKey, defaultValue: false);
        }

        public static void SetBlockExplicitSongsInRadio(this ISettingsService @this, bool value)
        {
            @this.SetApplicationValue(BlockExplicitSongsInRadioKey, value);
        }

        public static bool GetIsAllAccessAvailable(this ISettingsService @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            return @this.GetApplicationValue<bool>(IsAllAccessAvailableKey, defaultValue: false);
        }

        public static void SetIsAllAccessAvailable(this ISettingsService @this, bool value)
        {
            @this.SetApplicationValue(IsAllAccessAvailableKey, value);
        }


        public static bool GetIsSearchLocalOnly(this ISettingsService @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            return @this.GetApplicationValue<bool>(IsSearchLocalOnlyKey, defaultValue: false);
        }

        public static void SetIsSearchLocalOnly(this ISettingsService @this, bool value)
        {
            @this.SetApplicationValue(IsSearchLocalOnlyKey, value);
        }
    }
}
