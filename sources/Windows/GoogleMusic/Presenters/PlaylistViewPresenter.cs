// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Linq;
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
            this.AddToPlaylistCommand = new DelegateCommand(this.AddToPlaylist);
        }

        public DelegateCommand PlaySelectedSongCommand { get; private set; }

        public DelegateCommand RemoveFromPlaylistCommand { get; private set; }

        public DelegateCommand AddToPlaylistCommand { get; private set; }

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

        public override void OnNavigatedTo(NavigatedToEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);

            var playlist = eventArgs.Parameter as Playlist;

            if (playlist != null)
            {
                this.BindingModel = new PlaylistViewBindingModel(playlist);
                this.View.SelectedIndex = -1;
            }
            else
            {
                var song = eventArgs.Parameter as Song;
                if (song != null)
                {
                    this.View.SetIsLoading(true);
                    this.SearchAlbum(song).ContinueWith(
                        (t) =>
                            {
                                this.BindingModel = new PlaylistViewBindingModel(t.Result);
                                this.View.SelectedIndex = t.Result.Songs.IndexOf(song);
                                this.View.SetIsLoading(false);
                            },
                        TaskScheduler.FromCurrentSynchronizationContext());
                }
                else
                {
                    this.BindingModel = null;
                    this.Logger.Error("OnNavigatedTo: Playlist or song is null.");
                }
            }
        }

        public void AddSelectedSongToPlaylist(MusicPlaylist playlist)
        {
            var song = this.BindingModel.Songs[this.View.SelectedIndex];
            this.songsService.AddSongToPlaylistAsync(playlist, song).ContinueWith(
                t =>
                    {
                        if (t.IsCompleted && t.Result)
                        {
                            if (this.BindingModel.Playlist == playlist)
                            {
                                this.BindingModel.ReloadSongs();
                            }
                        }
                    },
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task<Playlist> SearchAlbum(Song song)
        {
            var albums = await this.songsService.GetAllAlbumsAsync();

            var album = albums.FirstOrDefault(x => x.Songs.Contains(song));

            return album;
        }

        private void PlaySelectedSong()
        {
            var selectedIndex = this.View.SelectedIndex;
            if (selectedIndex >= 0)
            {
                this.currentPlaylistService.ClearPlaylist();
                this.currentPlaylistService.SetPlaylist(this.BindingModel.Playlist);
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

        private void AddToPlaylist()
        {
            this.songsService.GetAllPlaylistsAsync(Order.Name).ContinueWith(
                t =>
                    {
                        if (t.IsCompleted)
                        {
                            this.View.ShowPlaylists(t.Result);
                        }
                    },
                TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}