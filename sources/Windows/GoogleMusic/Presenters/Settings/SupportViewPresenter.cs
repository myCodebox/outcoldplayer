// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Settings
{
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class SupportViewPresenter : DonatePresenterBase
    {
        private readonly ISettingsService settingsService;

        private readonly IAnalyticsService analyticsService;

        public SupportViewPresenter(
            ISettingsService settingsService,
            IApplicationSettingViewsService settingViewsService,
            IAnalyticsService analyticsService)
            : base(analyticsService)
        {
            this.settingsService = settingsService;
            this.analyticsService = analyticsService;

            this.ShowTutorial = new DelegateCommand(
                () =>
                {
                    this.analyticsService.SendEvent("Support", "Execute", "ShowTutorial");
                    settingViewsService.Close();
                    this.MainFrame.ShowPopup<ITutorialPopupView>(PopupRegion.Full);
                });
        }

        public DelegateCommand ShowTutorial { get; set; }

        public bool IsLoggingOn
        {
            get
            {
                return this.settingsService.GetValue("IsLoggingOn", false);
            }

            set
            {
                this.settingsService.SetValue("IsLoggingOn", value);
                this.RaiseCurrentPropertyChanged();
            }
        }
    }
}