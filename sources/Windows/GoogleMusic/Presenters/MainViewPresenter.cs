// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class MainViewPresenter : ViewPresenterBase<IMainView>
    {
        private readonly IDependencyResolverContainer container;
        private readonly IAuthentificationService authentificationService;
        private readonly IGoogleMusicSessionService sessionService;

        private readonly INavigationService navigationService;

        public MainViewPresenter(
            IDependencyResolverContainer container, 
            IAuthentificationService authentificationService,
            IGoogleMusicSessionService sessionService,
            INavigationService navigationService)
            : base(container)
        {
            this.container = container;
            this.authentificationService = authentificationService;
            this.sessionService = sessionService;
            this.navigationService = navigationService;
            this.BindingModel = new MainViewBindingModel
                                    {
                                        Message = "Signing in...", 
                                        IsProgressRingActive = true
                                    };

            this.Logger.Debug("Checking authentification.");
            this.authentificationService.CheckAuthentificationAsync().ContinueWith(
               task =>
                   {
                       if (task.IsCompleted && !task.IsFaulted && task.Result.Succeed)
                       {
                           this.BindingModel.IsAuthenticated = true;
                           this.Logger.Debug("User is logged in. Going to start view and showing player.");
                           this.navigationService.NavigateTo<IProgressLoadingView>(keepInHistory: false);
                       }
                       else
                       {
                           this.Logger.Debug("User is not logged in. Showing authentification view.");
                           this.navigationService.NavigateTo<IAuthentificationView>(keepInHistory: false).Succeed += this.AuthentificationViewOnSucceed;
                       }
                   },
               TaskScheduler.FromCurrentSynchronizationContext());

            this.sessionService.SessionCleared += (sender, args) => this.Dispatcher.RunAsync(
                () =>
                    {
                        App.Container.Resolve<ISearchService>().Unregister();
                        App.Container.Resolve<ISettingsCommands>().Unregister();
                        if (this.BindingModel.IsAuthenticated)
                        {
                            this.BindingModel.IsAuthenticated = false;
                            this.navigationService.ClearHistory();
                            this.navigationService.NavigateTo<IAuthentificationView>(keepInHistory: false).Succeed += this.AuthentificationViewOnSucceed;
                        }
                    });

            this.navigationService.NavigatedTo += (sender, args) =>
                {
                    this.UpdateCanGoBack();
                    this.BindingModel.Message = null;
                    this.BindingModel.IsProgressRingActive = false;
                };
        }

        public MainViewBindingModel BindingModel { get; private set; }

        public PlayerViewPresenter PlayerViewPresenter { get; private set; }

        public bool HasHistory()
        {
            return this.navigationService.HasHistory();
        }

        public void GoBack()
        {
            if (this.navigationService.CanGoBack())
            {
                this.navigationService.GoBack();
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var playerViewPresenter = this.container.Resolve<PlayerViewPresenter>(new object[] { this.View });
            ((IViewPresenterBase)playerViewPresenter).Initialize(this.View);
            this.PlayerViewPresenter = playerViewPresenter;
        }

        private void UpdateCanGoBack()
        {
            this.BindingModel.CanGoBack = this.navigationService.CanGoBack();
        }

        private void AuthentificationViewOnSucceed(object sender, EventArgs eventArgs)
        {
            this.Logger.Debug("Authentification view on succed.");

            ((IAuthentificationView)sender).Succeed -= this.AuthentificationViewOnSucceed;

            this.BindingModel.IsAuthenticated = true;
            this.navigationService.NavigateTo<IProgressLoadingView>(keepInHistory: false);
        }
    }
}