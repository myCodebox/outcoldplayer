// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;

    public class SnappedPlayerBindingModel : PlayerBindingModel
    {
        private readonly IMediaElementContainer mediaElementContainer;
        private readonly IPlayQueueService playQueueService;
        private readonly IEventAggregator eventAggregator;
        private readonly IDispatcher dispatcher;

        private IList<SongBindingModel> songs;

        public SnappedPlayerBindingModel(
            IMediaElementContainer mediaElementContainer,
            IPlayQueueService playQueueService,
            IEventAggregator eventAggregator,
            IDispatcher dispatcher)
        {
            this.mediaElementContainer = mediaElementContainer;
            this.playQueueService = playQueueService;
            this.eventAggregator = eventAggregator;
            this.dispatcher = dispatcher;

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
                return this.Songs == null || this.Songs.Count == 0;
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

        public IList<SongBindingModel> Songs
        {
            get
            {
                return this.songs;
            }

            set
            {
                this.SetValue(ref this.songs, value);
                this.RaiseCurrentPropertyChanged();
                this.RaisePropertyChanged(() => this.IsQueueEmpty);
            }
        }
    }
}
