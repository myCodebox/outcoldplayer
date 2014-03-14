// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.BindingModels.Popups
{
    using System;

    using OutcoldSolutions.GoogleMusic.EventAggregator;
    using OutcoldSolutions.GoogleMusic.Services;

    public class PlayerMorePopupViewBindingModel : DisposableBindingModelBase
    {
        private readonly IApplicationStateService stateService;
        private readonly IApplicationResources applicationResources;
        private readonly IDispatcher dispatcher;

        public PlayerMorePopupViewBindingModel(
            IEventAggregator eventAggregator,
            IApplicationStateService stateService,
            IApplicationResources applicationResources,
            IDispatcher dispatcher)
        {
            this.stateService = stateService;
            this.applicationResources = applicationResources;
            this.dispatcher = dispatcher;

            this.RegisterForDispose(eventAggregator.GetEvent<ApplicationStateChangeEvent>().Subscribe(
                async (e) => await this.dispatcher.RunAsync(() => this.RaisePropertyChanged(() => this.IsOnlineMode))));
        }

        public bool IsOnlineMode
        {
            get
            {
                return this.stateService.CurrentState == ApplicationState.Online;
            }

            set
            {
                this.stateService.CurrentState = value ? ApplicationState.Online : ApplicationState.Offline;
                this.RaisePropertyChanged(() => this.ApplicationStateText);
            }
        }

        public string ApplicationStateText
        {
            get
            {
                if (this.IsOnlineMode)
                {
                    return this.applicationResources.GetString("OnlineMode");
                }

                return this.applicationResources.GetString("OfflineMode");
            }
        }
    }
}
