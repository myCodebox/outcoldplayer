// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.BindingModels.Popups
{
    using System;

    using OutcoldSolutions.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;

    public class PlayerMorePopupViewBindingModel : DisposableBindingModelBase
    {
        private readonly IMediaElementContainer mediaElementContainer;
        private readonly IPlayQueueService playQueueService;
        private readonly IEventAggregator eventAggregator;
        private readonly IApplicationStateService stateService;
        private readonly IApplicationResources applicationResources;
        private readonly IDispatcher dispatcher;

        public PlayerMorePopupViewBindingModel(
            IMediaElementContainer mediaElementContainer,
            IPlayQueueService playQueueService,
            IEventAggregator eventAggregator,
            IApplicationStateService stateService,
            IApplicationResources applicationResources,
            IDispatcher dispatcher)
        {
            this.mediaElementContainer = mediaElementContainer;
            this.playQueueService = playQueueService;
            this.eventAggregator = eventAggregator;
            this.stateService = stateService;
            this.applicationResources = applicationResources;
            this.dispatcher = dispatcher;

            this.RegisterForDispose(this.eventAggregator.GetEvent<QueueChangeEvent>().Subscribe(
                async (e) => await this.dispatcher.RunAsync(() =>
                                {
                                    this.RaisePropertyChanged(() => this.IsShuffleEnabled);
                                    this.RaisePropertyChanged(() => this.IsRepeatAllEnabled);
                                })));

            this.RegisterForDispose(this.eventAggregator.GetEvent<ApplicationStateChangeEvent>().Subscribe(
                async (e) => await this.dispatcher.RunAsync(() => this.RaisePropertyChanged(() => this.IsOnlineMode))));
        }

        public bool IsShuffleEnabled
        {
            get
            {
                return this.playQueueService.IsShuffled;
            }

            set
            {
                this.playQueueService.IsShuffled = value;
            }
        }

        public bool IsRepeatAllEnabled
        {
            get
            {
                return this.playQueueService.IsRepeatAll;
            }

            set
            {
                this.playQueueService.IsRepeatAll = value;
            }
        }

        public double Volume
        {
            get
            {
                return this.mediaElementContainer.Volume;
            }

            set
            {
                this.mediaElementContainer.Volume = value;
            }
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
