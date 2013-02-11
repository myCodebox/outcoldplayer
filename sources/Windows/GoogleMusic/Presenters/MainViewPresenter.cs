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
            IMainView view,
            IAuthentificationService authentificationService,
            IGoogleMusicSessionService sessionService,
            INavigationService navigationService)
            : base(container, view)
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

            this.PlayerViewPresenter = this.container.Resolve<PlayerViewPresenter>(new object[] { view });

            this.sessionService.SessionCleared += (sender, args) => this.Dispatcher.RunAsync(
                () =>
                    {
                        App.Container.Resolve<ISearchService>().Unregister();
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