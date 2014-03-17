// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Settings
{
    using OutcoldSolutions.GoogleMusic.Services;

    public class SupportViewPresenter : DonatePresenterBase
    {
        private readonly ISettingsService settingsService;

        public SupportViewPresenter(
            ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }

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