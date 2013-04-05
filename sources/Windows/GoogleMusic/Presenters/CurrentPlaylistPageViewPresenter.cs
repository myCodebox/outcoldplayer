// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Controls;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    public class CurrentPlaylistPageViewPresenter : PagePresenterBase<ICurrentPlaylistPageView>
    {
        private readonly IPlayQueueService playQueueService;
        private readonly ISongsService metadataEditService;

        public CurrentPlaylistPageViewPresenter(
            IPlayQueueService playQueueService,
            ISongsService metadataEditService,
            SongsBindingModel songsBindingModel)
        {
            this.playQueueService = playQueueService;
            this.metadataEditService = metadataEditService;
            this.BindingModel = songsBindingModel;

            this.playQueueService.QueueChanged += async (sender, args) => await this.Dispatcher.RunAsync(this.UpdateSongs);

            //this.SaveAsPlaylistCommand = new DelegateCommand(this.SaveAsPlaylist, () => this.BindingModel.Songs.Count > 0);
            this.RemoveSelectedSongCommand = new DelegateCommand(this.RemoveSelectedSong);
            this.AddToPlaylistCommand = new DelegateCommand(this.AddToPlaylist);
            this.RateSongCommand = new DelegateCommand(this.RateSong);
        }

        public DelegateCommand AddToPlaylistCommand { get; private set; }

        public DelegateCommand SaveAsPlaylistCommand { get; private set; }

        public DelegateCommand RemoveSelectedSongCommand { get; set; }

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

        private void AddToPlaylist()
        {
            var selectedSongs = this.BindingModel.GetSelectedSongs().ToList();
            if (selectedSongs != null)
            {
                this.MainFrame.ShowPopup<IAddToPlaylistPopupView>(PopupRegion.AppToolBarLeft, selectedSongs);
            }
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

        private async void RemoveSelectedSong()
        {
            var collection = this.BindingModel.GetSelectedIndexes().ToList();
            if (collection.Count > 0)
            {
                await this.playQueueService.RemoveAsync(collection);

                await this.Dispatcher.RunAsync(
                    () =>
                        {
                            if (collection.Count == 1)
                            {
                                int selectedSongIndex = collection.First();
                                if (selectedSongIndex < this.BindingModel.Songs.Count)
                                {
                                    this.BindingModel.SelectSongByIndex(selectedSongIndex);
                                }
                                else if (this.BindingModel.Songs.Count > 0)
                                {
                                    this.BindingModel.SelectSongByIndex(selectedSongIndex - 1);
                                }
                            }
                        });
            }
        }

        private void UpdateSongs()
        {
            this.BindingModel.SetCollection(this.playQueueService.GetQueue());
        }

        private void SelectedSongChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            this.AddToPlaylistCommand.RaiseCanExecuteChanged();
            this.RemoveSelectedSongCommand.RaiseCanExecuteChanged();

            if (this.BindingModel.SelectedItems.Count > 0)
            {
                this.MainFrame.SetContextCommands(this.GetContextCommands());
            }
            else
            {
                this.MainFrame.ClearContextCommands();
            }
        }

        private IEnumerable<CommandMetadata> GetContextCommands()
        {
            yield return new CommandMetadata(CommandIcon.Add, "Playlist", this.AddToPlaylistCommand);
            yield return new CommandMetadata(CommandIcon.Remove, "Queue", this.RemoveSelectedSongCommand);
        }
    }
}