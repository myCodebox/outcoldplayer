// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;

    using OutcoldSolutions.Diagnostics;
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

            this.sessionService.SessionCleared += (sender, args) => this.Dispatcher.RunAsync(
                () =>
                {
                    container.Resolve<ISearchService>().Unregister();
                    container.Resolve<ISettingsCommands>().Unregister();

                    this.navigationService.ClearHistory();
                    this.navigationService.NavigateTo<IAuthentificationPageView>(keepInHistory: false);
                });
        }

        public override async void OnNavigatedTo(NavigatedToEventArgs parameter)
        {
            base.OnNavigatedTo(parameter);
            
            AuthentificationService.AuthentificationResult result = null;

            try
            {
                this.Logger.Debug("Verifying authentification.");
                result = await this.authentificationService.CheckAuthentificationAsync();
            }
            catch (Exception e)
            {
                this.Logger.LogErrorException(e);
            }

            if (result != null && result.Succeed)
            {
                this.Logger.Debug("User is logged in. Going to IProgressLoadingView.");
                this.navigationService.NavigateTo<IProgressLoadingView>(keepInHistory: false);
            }
            else
            {
                if (result != null)
                {
                    this.Logger.Debug("We got an error message from google services: '{0}'.", result.ErrorMessage);
                }

                this.Logger.Debug("User is not logged in. Going to IAuthentificationPageView.");
                this.navigationService.NavigateTo<IAuthentificationPageView>(keepInHistory: false);
            }
        }
    }
}