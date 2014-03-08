// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Controls;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class CurrentPlaylistPageViewPresenter : PagePresenterBase<ICurrentPlaylistPageView>
    {
        private readonly IPlayQueueService playQueueService;
        private readonly ISongsService metadataEditService;

        private readonly ISelectedObjectsService selectedObjectsService;

        internal CurrentPlaylistPageViewPresenter(
            IPlayQueueService playQueueService,
            ISongsService metadataEditService,
            SongsBindingModel songsBindingModel,
            ISelectedObjectsService selectedObjectsService)
        {
            this.playQueueService = playQueueService;
            this.metadataEditService = metadataEditService;
            this.selectedObjectsService = selectedObjectsService;
            this.BindingModel = songsBindingModel;

            this.playQueueService.QueueChanged += async (sender, args) => await this.Dispatcher.RunAsync(this.UpdateSongs);

            //this.SaveAsPlaylistCommand = new DelegateCommand(this.SaveAsPlaylist, () => this.BindingModel.Songs.Count > 0);
            this.RateSongCommand = new DelegateCommand(this.RateSong);

            this.playQueueService.StateChanged += async (sender, args) => await this.Dispatcher.RunAsync(async () => 
                {
                    if (this.BindingModel.SelectedItems.Count == 0)
                    {
                        if (this.BindingModel.Songs != null && args.CurrentSong != null)
                        {
                            var currentSong = this.BindingModel.Songs.FirstOrDefault(x => string.Equals(x.Metadata.SongId, args.CurrentSong.SongId, StringComparison.Ordinal));
                            if (currentSong != null)
                            {
                                await this.View.ScrollIntoCurrentSongAsync(currentSong);
                            }
                        }
                    }
                });
        }

        public DelegateCommand SaveAsPlaylistCommand { get; private set; }

        public DelegateCommand RateSongCommand { get; set; }

        public SongsBindingModel BindingModel { get; set; }

        public void SelectPlayingSong()
        {
            this.BindingModel.ClearSelectedItems();
            this.BindingModel.SelectSongByIndex(this.playQueueService.GetCurrentSongIndex());
        }

        public void PlaySong(SongBindingModel songBindingModel)
        {
            int songIndex = this.BindingModel.Songs.IndexOf(songBindingModel);
            this.Logger.LogTask(this.playQueueService.PlayAsync(songIndex));
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.BindingModel.SelectedItems.CollectionChanged += this.SelectedSongChanged;

            this.EventAggregator.GetEvent<SelectionClearedEvent>()
               .Subscribe<SelectionClearedEvent>(async (e) => await this.Dispatcher.RunAsync(() => this.BindingModel.ClearSelectedItems()));
        }

        //protected override IEnumerable<CommandMetadata> GetViewCommands()
        //{
        //    yield return new CommandMetadata(CommandIcon.Page, "Save", this.SaveAsPlaylistCommand);
        //}

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            await this.Dispatcher.RunAsync(
                () =>
                    {
                        this.UpdateSongs();

                        if (navigatedToEventArgs.Parameter is bool && (bool)navigatedToEventArgs.Parameter)
                        {
                            this.SelectPlayingSong();
                        }
                    });
        }

        private void RateSong(object parameter)
        {
            var ratingEventArgs = parameter as RatingEventArgs;
            Debug.Assert(ratingEventArgs != null, "ratingEventArgs != null");
            if (ratingEventArgs != null)
            {
                this.Logger.LogTask(this.metadataEditService.UpdateRatingAsync(
                        ((SongBindingModel)ratingEventArgs.CommandParameter).Metadata, (byte)ratingEventArgs.Value));
            }
        }

        private void UpdateSongs()
        {
            this.BindingModel.SetCollection(this.playQueueService.GetQueue());
        }

        private void SelectedSongChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.selectedObjectsService.Update(
                e.NewItems == null ? null : e.NewItems.Cast<SongBindingModel>().Select(x => x.Metadata),
                e.OldItems == null ? null : e.OldItems.Cast<SongBindingModel>().Select(x => x.Metadata));
        }
    }
}