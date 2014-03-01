// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System;

    using OutcoldSolutions.GoogleMusic.Services;

    public static class GoogleMusicSettingsServiceExtensions
    {
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
    }
}
