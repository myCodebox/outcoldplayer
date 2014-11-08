// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Settings
{
    using System.Collections.Generic;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class AppSettingsViewPresenter : ViewPresenterBase<IView>
    {
        private readonly ISettingsService settingsService;

        private readonly IAnalyticsService analyticsService;
        private readonly IMainFrame mainFrame;
        private readonly IApplicationSettingViewsService applicationSettingViewsService;

        private bool busy = false;

        public AppSettingsViewPresenter(
            ISettingsService settingsService,
            IAnalyticsService analyticsService,
            IMainFrame mainFrame,
            IApplicationSettingViewsService applicationSettingViewsService)
        {
            this.settingsService = settingsService;
            this.analyticsService = analyticsService;
            this.mainFrame = mainFrame;
            this.applicationSettingViewsService = applicationSettingViewsService;
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

        public bool IsMusicLibraryForCache
        {
            get
            {
                return this.settingsService.GetIsMusicLibraryForCache();
            }

            set
            {
                if (!this.busy)
                {
                    this.busy = true;
                    this.mainFrame.ShowPopup<ICacheMovePopupView>(PopupRegion.Full, value);
                    this.applicationSettingViewsService.Close();
                }
            }
        }

        public bool IsThumbsRating
        {
            get
            {
                return this.settingsService.GetIsThumbsRating();
            }

            set
            {
                this.analyticsService.SendEvent("Settings", "ChangeIsThumbsRating", value.ToString());
                this.settingsService.SetIsThumbsRating(value);
            }
        }
    }
}