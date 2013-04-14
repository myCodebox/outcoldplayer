// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels.Settings
{
    using OutcoldSolutions.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;

    public class SupportBindingModel : BindingModelBase
    {
        private readonly ISettingsService settingsService;

        public SupportBindingModel()
        {
            this.settingsService = App.Container.Resolve<ISettingsService>();
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