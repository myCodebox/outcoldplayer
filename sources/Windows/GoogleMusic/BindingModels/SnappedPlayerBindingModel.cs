// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;

    public class SnappedPlayerBindingModel : PlayerBindingModel
    {
        private readonly IMediaElementContainer mediaElementContainer;
        private readonly IPlayQueueService playQueueService;
        private readonly IEventAggregator eventAggregator;
        private readonly IDispatcher dispatcher;

        private bool isQueueEmpty = true;

        public SnappedPlayerBindingModel(
            SongsBindingModel songsBindingModel,
            IMediaElementContainer mediaElementContainer,
            IPlayQueueService playQueueService,
            IEventAggregator eventAggregator,
            IDispatcher dispatcher)
        {
            this.mediaElementContainer = mediaElementContainer;
            this.playQueueService = playQueueService;
            this.eventAggregator = eventAggregator;
            this.dispatcher = dispatcher;
            this.SongsBindingModel = songsBindingModel;

            this.eventAggregator.GetEvent<QueueChangeEvent>().Subscribe(
                async (e) => await this.dispatcher.RunAsync(() =>
                                {
                                    this.RaisePropertyChanged(() => this.IsShuffleEnabled);
                                    this.RaisePropertyChanged(() => this.IsRepeatAllEnabled);
                                }));
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

        public bool IsQueueEmpty
        {
            get
            {
                return this.isQueueEmpty;
            }

            set
            {
                this.SetValue(ref this.isQueueEmpty, value);
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

        public SongsBindingModel SongsBindingModel { get; private set; }
    }
}
