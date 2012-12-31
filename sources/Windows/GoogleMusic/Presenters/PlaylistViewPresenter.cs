// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class PlaylistViewPresenter : ViewPresenterBase<IPlaylistView>
    {
        private readonly ICurrentPlaylistService currentPlaylistService;

        private readonly ISongsService songsService;

        private PlaylistViewBindingModel bindingModel;

        public PlaylistViewPresenter(
            IDependencyResolverContainer container, 
            IPlaylistView view,
            ICurrentPlaylistService currentPlaylistService,
            ISongsService songsService)
            : base(container, view)
        {
            this.currentPlaylistService = currentPlaylistService;
            this.songsService = songsService;
            this.PlaySelectedSongCommand = new DelegateCommand(this.PlaySelectedSong);
            this.RemoveFromPlaylistCommand = new DelegateCommand(this.RemoveFromPlaylist);
        }

        public DelegateCommand PlaySelectedSongCommand { get; private set; }

        public DelegateCommand RemoveFromPlaylistCommand { get; private set; }

        public PlaylistViewBindingModel BindingModel
        {
            get
            {
                return this.bindingModel;
            }

            private set
            {
                if (this.bindingModel != value)
                {
                    this.bindingModel = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }

        public override void OnNavigatedTo(object parameter)
        {
            base.OnNavigatedTo(parameter);

            var playlist = parameter as Playlist;
            if (playlist != null)
            {
                this.BindingModel = new PlaylistViewBindingModel(playlist);
            }
            else
            {
                this.BindingModel = null;
                this.Logger.Error("OnNavigatedTo: Playlist it null.");
            }
        }

        private void PlaySelectedSong()
        {
            var selectedIndex = this.View.SelectedIndex;
            if (selectedIndex >= 0)
            {
                this.currentPlaylistService.ClearPlaylist();
                this.currentPlaylistService.AddSongs(this.BindingModel.Songs);
                this.currentPlaylistService.PlayAsync(selectedIndex);
            }
        }

        private void RemoveFromPlaylist()
        {
            if (this.BindingModel != null && !this.BindingModel.IsBusy)
            {
                this.BindingModel.IsBusy = true;
                var selectedIndex = this.View.SelectedIndex;
                var musicPlaylist = (MusicPlaylist)this.BindingModel.Playlist;

                this.songsService.RemoveSongFromPlaylistAsync(
                    musicPlaylist, selectedIndex).ContinueWith(
                        t =>
                            {
                                this.BindingModel.Songs.RemoveAt(selectedIndex);
                                this.BindingModel.IsBusy = false;
                                if (this.BindingModel.Songs.Count > 0)
                                {
                                    if (this.BindingModel.Songs.Count <= selectedIndex)
                                    {
                                        selectedIndex--;
                                    }

                                    this.View.SelectedIndex = selectedIndex;
                                }
                            },
                        TaskScheduler.FromCurrentSynchronizationContext());
            }
        }
    }
}