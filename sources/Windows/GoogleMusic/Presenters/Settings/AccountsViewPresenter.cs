// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Settings
{
    using OutcoldSolutions.GoogleMusic.BindingModels.Settings;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Services.Publishers;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.GoogleMusic.Web.Lastfm;
    using OutcoldSolutions.GoogleMusic.Web.Synchronization;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Shell;
    using OutcoldSolutions.Views;

    public class AccountsViewPresenter : ViewPresenterBase<IView>
    {
        private readonly IGoogleAccountService googleAccountService;
        private readonly IGoogleMusicSessionService sessionService;
        private readonly ILastfmWebService lastfmWebService;
        private readonly ICurrentSongPublisherService publisherService;
        private readonly ILastFmConnectionService lastFmConnectionService;
        private readonly IApplicationSettingViewsService applicationSettingViewsService;
        private readonly INavigationService navigationService;

        private readonly IGoogleMusicSynchronizationService synchronizationService;

        public AccountsViewPresenter(
            IGoogleAccountService googleAccountService,
            IGoogleMusicSessionService sessionService,
            ILastfmWebService lastfmWebService,
            ICurrentSongPublisherService publisherService,
            ILastFmConnectionService lastFmConnectionService,
            IApplicationSettingViewsService applicationSettingViewsService,
            INavigationService navigationService,
            IGoogleMusicSynchronizationService synchronizationService)
        {
            this.googleAccountService = googleAccountService;
            this.sessionService = sessionService;
            this.lastfmWebService = lastfmWebService;
            this.publisherService = publisherService;
            this.lastFmConnectionService = lastFmConnectionService;
            this.applicationSettingViewsService = applicationSettingViewsService;
            this.navigationService = navigationService;
            this.synchronizationService = synchronizationService;
            this.BindingModel = new AccountViewBindingModel();
            this.ForgetAccountCommand = new DelegateCommand(this.ForgetAccount);
            this.SignOutCommand = new DelegateCommand(this.SignOutAccount);
            this.LastfmUnlinkCommand = new DelegateCommand(this.LastfmUnlink);
            this.LastfmLinkCommand = new DelegateCommand(this.LastfmLink);
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
                this.BindingModel.Message =
                    "Username and password were cleared. You are still signed in, on next application start gMusic can still use your Token and Session keys, to delete them click on Sign Out button.";
            }
            else
            {
                this.BindingModel.Message = "Username and password were cleared.";
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
            this.lastFmConnectionService.Connect();
        }

        private async void ReloadSongs()
        {
            await this.synchronizationService.ClearLocalDatabaseAsync();

            this.navigationService.ClearHistory();

            this.navigationService.NavigateTo<IProgressLoadingView>(keepInHistory: false);

            this.applicationSettingViewsService.Close();
        }
    }
}