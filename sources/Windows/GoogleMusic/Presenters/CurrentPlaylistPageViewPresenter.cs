// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Controls;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Presenters;

    public class CurrentPlaylistPageViewPresenter : DataPagePresenterBase<ICurrentPlaylistPageView, CurrentPlaylistPageViewBindingModel>
    {
        private readonly IPlayQueueService playQueueService;
        private readonly ISongMetadataEditService metadataEditService;

        public CurrentPlaylistPageViewPresenter(
            IPlayQueueService playQueueService,
            ISongMetadataEditService metadataEditService)
        {
            this.playQueueService = playQueueService;
            this.metadataEditService = metadataEditService;

            this.playQueueService.QueueChanged += (sender, args) => this.UpdateSongs();

            this.PlaySelectedSongCommand = new DelegateCommand(this.PlaySelectedSong);
            this.RemoveSelectedSongCommand = new DelegateCommand(this.RemoveSelectedSong);
            this.AddToPlaylistCommand = new DelegateCommand(this.AddToPlaylist);
            this.RateSongCommand = new DelegateCommand(this.RateSong);
        }

        public DelegateCommand AddToPlaylistCommand { get; private set; }

        public DelegateCommand PlaySelectedSongCommand { get; set; }

        public DelegateCommand RemoveSelectedSongCommand { get; set; }

        public DelegateCommand RateSongCommand { get; set; }

        public void SelectPlayingSong()
        {
            this.BindingModel.SelectedSongIndex = this.playQueueService.GetCurrentSongIndex();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.BindingModel.Subscribe(() => this.BindingModel.SelectedSong, this.SelectedSongChanged);
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            await Task.Run(() =>
                {
                    this.UpdateSongs();
                    this.SelectPlayingSong();
                });
        }

        private void AddToPlaylist()
        {
            var selectedSong = this.BindingModel.Songs[this.BindingModel.SelectedSongIndex];
            if (selectedSong != null)
            {
                this.Toolbar.ShowPopup<IAddToPlaylistPopupView>(new List<Song>{ selectedSong });
            }
        }

        private void RateSong(object parameter)
        {
            var ratingEventArgs = parameter as RatingEventArgs;
            Debug.Assert(ratingEventArgs != null, "ratingEventArgs != null");
            if (ratingEventArgs != null)
            {
                this.Logger.LogTask(this.metadataEditService.UpdateRatingAsync(
                        (Song)ratingEventArgs.CommandParameter, (byte)ratingEventArgs.Value));
            }
        }

        private async void RemoveSelectedSong()
        {
            var selectedSongIndex = this.BindingModel.SelectedSongIndex;
            if (selectedSongIndex >= 0)
            {
                await this.playQueueService.RemoveAsync(selectedSongIndex);

                if (selectedSongIndex < this.BindingModel.Songs.Count)
                {
                    this.BindingModel.SelectedSongIndex = selectedSongIndex;
                }
                else if (this.BindingModel.Songs.Count > 0)
                {
                    this.BindingModel.SelectedSongIndex = selectedSongIndex - 1;
                }

                this.UpdateSongs();
            }
        }

        private void PlaySelectedSong()
        {
            var selectedSongIndex = this.BindingModel.SelectedSongIndex;
            if (selectedSongIndex >= 0)
            {
                this.Logger.LogTask(this.playQueueService.PlayAsync(selectedSongIndex));
            }
        }

        private void UpdateSongs()
        {
            this.Dispatcher.RunAsync(() => { this.BindingModel.Songs = new List<Song>(this.playQueueService.GetQueue()); });
        }

        private void SelectedSongChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            this.PlaySelectedSongCommand.RaiseCanExecuteChanged();
            this.AddToPlaylistCommand.RaiseCanExecuteChanged();
            this.RemoveSelectedSongCommand.RaiseCanExecuteChanged();

            if (this.BindingModel.SelectedSong != null)
            {
                this.Toolbar.SetContextCommands(this.GetContextCommands());
            }
            else
            {
                this.Toolbar.ClearContextCommands();
            }
        }

        private IEnumerable<CommandMetadata> GetContextCommands()
        {
            yield return new CommandMetadata(CommandIcon.Play, "Play", this.PlaySelectedSongCommand);
            yield return new CommandMetadata(CommandIcon.Add, "Playlist", this.AddToPlaylistCommand);
            yield return new CommandMetadata(CommandIcon.Remove, "Queue", this.RemoveSelectedSongCommand);
        }
    }
}