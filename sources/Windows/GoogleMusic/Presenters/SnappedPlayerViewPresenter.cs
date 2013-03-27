// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views;

    using Windows.UI.ViewManagement;

    public class SnappedPlayerViewPresenter : PlayerViewPresenterBase<SnappedPlayerBindingModel, ISnappedPlayerView>
    {
        private readonly IPlayQueueService queueService;

        public SnappedPlayerViewPresenter(
            IMediaElementContainer mediaElementContainer, 
            IGoogleMusicSessionService sessionService, 
            IPlayQueueService queueService, 
            INavigationService navigationService,
            SnappedPlayerBindingModel snappedPlayerBindingModel)
            : base(mediaElementContainer, sessionService, queueService, navigationService, snappedPlayerBindingModel)
        {
            this.queueService = queueService;
            this.BindingModel.IsRepeatAllEnabled = this.queueService.IsRepeatAll;
            this.BindingModel.IsShuffleEnabled = this.queueService.IsShuffled;
            this.BindingModel.IsQueueEmpty = !this.queueService.GetQueue().Any();

            this.RepeatAllCommand =
                new DelegateCommand(
                    async () => await this.queueService.SetRepeatAllAsync(!this.queueService.IsRepeatAll),
                    () => this.queueService.State != QueueState.Busy);

            this.ShuffleCommand =
                new DelegateCommand(
                    async () => await this.queueService.SetShuffledAsync(!this.queueService.IsShuffled),
                    () => this.queueService.State != QueueState.Busy);

            this.AddToQueueCommand = new DelegateCommand(async () => await this.Dispatcher.RunAsync(() =>
                {
                    if (ApplicationView.TryUnsnap())
                    {
                        navigationService.NavigateTo<IStartPageView>();
                    }
                }));

            this.queueService.StateChanged += (sender, args) =>
                {
                    this.RepeatAllCommand.RaiseCanExecuteChanged();
                    this.ShuffleCommand.RaiseCanExecuteChanged();
                };
        }

        public DelegateCommand ShuffleCommand { get; set; }

        public DelegateCommand RepeatAllCommand { get; set; }

        public DelegateCommand AddToQueueCommand { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.EventAggregator.GetEvent<QueueChangeEvent>().Subscribe(
                async (e) =>
                    {
                        await this.Dispatcher.RunAsync(
                            () =>
                                {
                                    this.BindingModel.IsRepeatAllEnabled = e.IsRepeatAllEnabled;
                                    this.BindingModel.IsShuffleEnabled = e.IsShuffleEnabled;
                                    this.BindingModel.IsQueueEmpty = !this.queueService.GetQueue().Any();
                                });
                    });
        }


    }
}
