// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Settings
{
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    public class AppSettingsViewPresenter : ViewPresenterBase<IView>
    {
        private readonly ISettingsService settingsService;

        public AppSettingsViewPresenter(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }

        public bool IsScreenOn
        {
            get
            {
                return this.settingsService.GetIsLockScreenEnabled();
            }

            set
            {
                this.settingsService.SetIsLockScreenEnabled(value);
            }
        }
    }
}