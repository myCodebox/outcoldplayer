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

        public static bool GetBlockExplicitSongsInRadio(this ISettingsService @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            return @this.GetValue<bool>(BlockExplicitSongsInRadioKey, defaultValue: false);
        }

        public static void SetBlockExplicitSongsInRadio(this ISettingsService @this, bool value)
        {
            @this.SetValue(BlockExplicitSongsInRadioKey, value);
        }
    }
}
