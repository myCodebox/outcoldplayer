// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.Presenters;

    public class InitPageViewPresenter : PagePresenterBase<IInitPageView>
    {
        private readonly IAuthentificationService authentificationService;
        private readonly INavigationService navigationService;
        private readonly IGoogleMusicSessionService sessionService;

        public InitPageViewPresenter(
            IDependencyResolverContainer container,
            IAuthentificationService authentificationService,
            INavigationService navigationService,
            IGoogleMusicSessionService sessionService)
            : base(container)
        {
            this.authentificationService = authentificationService;
            this.navigationService = navigationService;
            this.sessionService = sessionService;
            this.BindingModel = new InitPageViewBindingModel();

            this.sessionService.SessionCleared += (sender, args) => this.Dispatcher.RunAsync(
                () =>
                {
                    container.Resolve<ISearchService>().Unregister();
                    container.Resolve<ISettingsCommands>().Unregister();

                    this.navigationService.ClearHistory();
                    this.navigationService.NavigateTo<IAuthentificationView>(keepInHistory: false);
                });
        }

        public InitPageViewBindingModel BindingModel { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs parameter)
        {
            base.OnNavigatedTo(parameter);

            this.BindingModel.Message = "Signing in...";

            this.Logger.Debug("Checking authentification.");
            this.authentificationService.CheckAuthentificationAsync().ContinueWith(
                task =>
                    {
                        if (task.IsCompleted && !task.IsFaulted && task.Result.Succeed)
                        {
                            this.Logger.Debug("User is logged in. Going to start view and showing player.");
                            this.navigationService.NavigateTo<IProgressLoadingView>(keepInHistory: false);
                        }
                        else
                        {
                            this.Logger.Debug("User is not logged in. Showing authentification view.");
                            this.navigationService.NavigateTo<IAuthentificationView>(keepInHistory: false);
                        }
                    },
                TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}