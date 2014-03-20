// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Settings
{
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class SupportViewPresenter : DonatePresenterBase
    {
        private readonly ISettingsService settingsService;

        public SupportViewPresenter(
            ISettingsService settingsService,
            IApplicationSettingViewsService settingViewsService)
        {
            this.settingsService = settingsService;

            this.ShowTutorial = new DelegateCommand(
                () =>
                {
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