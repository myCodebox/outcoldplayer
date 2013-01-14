// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Settings
{
    using OutcoldSolutions.GoogleMusic.BindingModels.Settings;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Services.Publishers;
    using OutcoldSolutions.GoogleMusic.Views.Settings;
    using OutcoldSolutions.GoogleMusic.Web.Lastfm;

    public class AccountViewPresenter : ViewPresenterBase<ISettingsView>
    {
        private readonly IGoogleAccountService googleAccountService;
        private readonly IGoogleMusicSessionService sessionService;
        private readonly ILastfmWebService lastfmWebService;
        private readonly ICurrentSongPublisherService publisherService;
        private readonly ISettingsCommands settingsCommands;

        public AccountViewPresenter(
            IDependencyResolverContainer container,
            ISettingsView view,
            IGoogleAccountService googleAccountService,
            IGoogleMusicSessionService sessionService,
            ILastfmWebService lastfmWebService,
            ICurrentSongPublisherService publisherService,
            ISettingsCommands settingsCommands)
            : base(container, view)
        {
            this.googleAccountService = googleAccountService;
            this.sessionService = sessionService;
            this.lastfmWebService = lastfmWebService;
            this.publisherService = publisherService;
            this.settingsCommands = settingsCommands;
            this.BindingModel = new AccountViewBindingModel();
            this.ForgetAccountCommand = new DelegateCommand(this.ForgetAccount);
            this.SignOutCommand = new DelegateCommand(this.SignOutAccount);
            this.LastfmUnlinkCommand = new DelegateCommand(this.LastfmUnlink);
            this.LastfmLinkCommand = new DelegateCommand(this.LastfmLink);

            var userInfo = this.googleAccountService.GetUserInfo();
            if (userInfo != null)
            {
                this.BindingModel.AccountName = userInfo.Email;
                this.BindingModel.IsRemembered = userInfo.RememberAccount;
            }

            Session lastfmSession = this.lastfmWebService.GetSession();
            if (lastfmSession != null)
            {
                this.BindingModel.LastfmAccountName = lastfmSession.Name;
            }

            this.BindingModel.HasSession = this.sessionService.GetSession().IsAuthenticated;
        }

        public AccountViewBindingModel BindingModel { get; private set; }

        public DelegateCommand ForgetAccountCommand { get; private set; }

        public DelegateCommand SignOutCommand { get; private set; }

        public DelegateCommand LastfmUnlinkCommand { get; private set; }

        public DelegateCommand LastfmLinkCommand { get; private set; }

        private void ForgetAccount()
        {
            this.googleAccountService.ClearUserInfo();
            this.BindingModel.AccountName = null;
            
            if (this.BindingModel.HasSession)
            {
                this.BindingModel.Message =
                    "Username and password were cleared. You are still signed in, on next application start gMusic can still use your Token and Session keys, to delete them click on Sign Out button.";
            }
            else
            {
                this.BindingModel.Message = "Username and password were cleared.";
            }
        }

        private void SignOutAccount()
        {
            this.sessionService.ClearSession();
            this.View.Hide();
        }

        private void LastfmUnlink()
        {
            this.lastfmWebService.ForgetAccount();
            this.publisherService.RemovePublishers<LastFmCurrentSongPublisher>();
            this.View.Hide();
        }

        private void LastfmLink()
        {
            this.View.Hide();
            this.settingsCommands.ActivateSettings("link-lastfm");
        }
    }
}