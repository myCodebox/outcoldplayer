// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Settings
{
    using OutcoldSolutions.GoogleMusic.BindingModels.Settings;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Services.Publishers;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.GoogleMusic.Web.Lastfm;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Shell;
    using OutcoldSolutions.Views;

    public class AccountsViewPresenter : ViewPresenterBase<IView>
    {
        private readonly IApplicationResources resources;
        private readonly IGoogleAccountService googleAccountService;
        private readonly IGoogleMusicSessionService sessionService;
        private readonly ILastfmWebService lastfmWebService;
        private readonly ICurrentSongPublisherService publisherService;
        private readonly IApplicationSettingViewsService applicationSettingViewsService;
        private readonly INavigationService navigationService;

        public AccountsViewPresenter(
            IApplicationResources resources,
            IGoogleAccountService googleAccountService,
            IGoogleMusicSessionService sessionService,
            ILastfmWebService lastfmWebService,
            ICurrentSongPublisherService publisherService,
            IApplicationSettingViewsService applicationSettingViewsService,
            INavigationService navigationService)
        {
            this.resources = resources;
            this.googleAccountService = googleAccountService;
            this.sessionService = sessionService;
            this.lastfmWebService = lastfmWebService;
            this.publisherService = publisherService;
            this.applicationSettingViewsService = applicationSettingViewsService;
            this.navigationService = navigationService;
            this.BindingModel = new AccountViewBindingModel();
            this.ForgetAccountCommand = new DelegateCommand(this.ForgetAccount);
            this.SignOutCommand = new DelegateCommand(this.SignOutAccount);
            this.LastfmUnlinkCommand = new DelegateCommand(this.LastfmUnlink);
            this.LastfmLinkCommand = new DelegateCommand(this.LastfmLink, () => this.sessionService.GetSession().IsAuthenticated);
            this.ReloadSongsCommand = new DelegateCommand(this.ReloadSongs, () => this.navigationService.HasHistory());

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

        public DelegateCommand ReloadSongsCommand { get; private set; }

        private void ForgetAccount()
        {
            this.googleAccountService.ClearUserInfo();
            this.BindingModel.AccountName = null;
            
            if (this.BindingModel.HasSession)
            {
                this.BindingModel.Message = this.resources.GetString("SettingsAccount_AccountClearedButYouAreSignedIn");
            }
            else
            {
                this.BindingModel.Message = this.resources.GetString("SettingsAccount_AccountCleared");
            }

            this.BindingModel.IsRemembered = false;
        }

        private void SignOutAccount()
        {
            this.sessionService.ClearSession();
            this.applicationSettingViewsService.Close();
        }

        private void LastfmUnlink()
        {
            this.lastfmWebService.ForgetAccount();
            this.publisherService.RemovePublishers<LastFmCurrentSongPublisher>();
            this.applicationSettingViewsService.Close();
        }

        private void LastfmLink()
        {
            this.applicationSettingViewsService.Close();
            this.MainFrame.ShowPopup<ILastfmAuthentificationView>(PopupRegion.Full);
        }

        private void ReloadSongs()
        {
            this.EventAggregator.Publish(new ReloadSongsEvent());
            this.applicationSettingViewsService.Close();
        }
    }
}