// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Settings
{
    using System.Collections;
    using System.Collections.Generic;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class AppSettingsViewPresenter : ViewPresenterBase<IView>
    {
        private readonly ISettingsService settingsService;

        private readonly IAnalyticsService analyticsService;

        public AppSettingsViewPresenter(
            ISettingsService settingsService,
            IAnalyticsService analyticsService)
        {
            this.settingsService = settingsService;
            this.analyticsService = analyticsService;
        }

        public bool IsScreenOn
        {
            get
            {
                return this.settingsService.GetIsLockScreenEnabled();
            }

            set
            {
                this.analyticsService.SendEvent("Settings", "ChangeIsScreenOn", value.ToString());
                this.settingsService.SetIsLockScreenEnabled(value);
            }
        }

        public bool BlockExplicitSongsInRadio
        {
            get
            {
                return this.settingsService.GetBlockExplicitSongsInRadio();
            }

            set
            {
                this.analyticsService.SendEvent("Settings", "ChangeBlockExplicitSongsInRadio", value.ToString());
                this.settingsService.SetBlockExplicitSongsInRadio(value);
            }
        }

        public IList<uint> Bitrates
        {
            get { return this.settingsService.GetStreamBitrates(); }
        }

        public uint SelectedBitrate
        {
            get
            {
                return this.settingsService.GetStreamBitrate();
            }

            set
            {
                this.analyticsService.SendEvent("Settings", "ChangeStreamBitrate", value.ToString());
                this.settingsService.SetStreamBitrate(value);
            }
        }
    }
}