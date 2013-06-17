// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Linq;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views;

    using Windows.UI.ViewManagement;

    public class SnappedPlayerViewPresenter : PlayerViewPresenterBase<SnappedPlayerBindingModel, ISnappedPlayerView>
    {
        private readonly IPlayQueueService queueService;

        private bool isRadio = false;

        public SnappedPlayerViewPresenter(
            IMediaElementContainer mediaElementContainer, 
            IGoogleMusicSessionService sessionService, 
            IPlayQueueService queueService, 
            INavigationService navigationService,
            SnappedPlayerBindingModel snappedPlayerBindingModel)
            : base(mediaElementContainer, sessionService, queueService, navigationService, snappedPlayerBindingModel)
        {
            this.queueService = queueService;
            this.isRadio = this.queueService.IsRadio;

            this.RepeatAllCommand =
                new DelegateCommand(
                    () => { },
                    () => this.queueService.State != QueueState.Busy && !this.isRadio);

            this.ShuffleCommand =
                new DelegateCommand(
                    () => { },
                    () => this.queueService.State != QueueState.Busy && !this.isRadio);

            this.AddToQueueCommand = new DelegateCommand(
                () =>
                    {
                        if (ApplicationView.TryUnsnap())
                        {
                            navigationService.NavigateTo<IStartPageView>();
                        }
                    });

            this.queueService.StateChanged += async (sender, args) => await this.Dispatcher.RunAsync(async () => 
                {
                    this.UpdateCommands();

                    await this.View.ScrollIntoCurrentSongAsync();
                });
        }

        public DelegateCommand ShuffleCommand { get; set; }

        public DelegateCommand RepeatAllCommand { get; set; }

        public DelegateCommand AddToQueueCommand { get; set; }

        public void PlaySong(SongBindingModel songBindingModel)
        {
            this.Logger.LogTask(this.queueService.PlayAsync(this.BindingModel.SongsBindingModel.Songs.IndexOf(songBindingModel)));
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.EventAggregator.GetEvent<QueueChangeEvent>()
                .Subscribe(
                    async (e) => await this.Dispatcher.RunAsync(
                        () =>
                            {
                                this.isRadio = e.IsRadio;

                                this.BindingModel.SongsBindingModel.SetCollection(e.SongsQueue);
                                this.BindingModel.IsQueueEmpty = this.BindingModel.SongsBindingModel.Songs == null
                                                                 || this.BindingModel.SongsBindingModel.Songs.Count == 0;

                                this.UpdateCommands();
                            }));

            this.EventAggregator.GetEvent<SongsUpdatedEvent>()
                           .Subscribe(async (e) => await this.Dispatcher.RunAsync(() =>
                               {
                                   if (this.BindingModel.CurrentSong != null)
                                   {
                                       var currentSongUpdated =
                                           e.UpdatedSongs.FirstOrDefault(
                                               s =>
                                               string.Equals(
                                                   s.ProviderSongId,
                                                   this.BindingModel.CurrentSong.Metadata.ProviderSongId,
                                                   StringComparison.Ordinal));

                                       if (currentSongUpdated != null)
                                       {
                                           this.BindingModel.CurrentSong.Metadata = currentSongUpdated;
                                       }
                                   }
                               }));
        }

        private void UpdateCommands()
        {
            this.RepeatAllCommand.RaiseCanExecuteChanged();
            this.ShuffleCommand.RaiseCanExecuteChanged();
        }
    }
}
