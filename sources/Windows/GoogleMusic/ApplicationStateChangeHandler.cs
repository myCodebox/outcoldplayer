//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using OutcoldSolutions.GoogleMusic.Services;

    internal class ApplicationStateChangeHandler
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IDispatcher dispatcher;
        private readonly INavigationService navigationService;

        public ApplicationStateChangeHandler(
            IEventAggregator eventAggregator,
            IDispatcher dispatcher,
            INavigationService navigationService)
        {
            this.eventAggregator = eventAggregator;
            this.dispatcher = dispatcher;
            this.navigationService = navigationService;

            this.eventAggregator.GetEvent<ApplicationStateChangeEvent>().Subscribe(this.OnApplicationStateChangeHandler);
        }

        private async void OnApplicationStateChangeHandler(ApplicationStateChangeEvent o)
        {
            await this.dispatcher.RunAsync(() =>
            {
                this.navigationService.ClearHistory();
                this.navigationService.RefreshCurrentView();
            });
        }
    }
}
