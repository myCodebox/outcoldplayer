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

    public class CurrentPlaylistViewPresenter : ViewPresenterBase<ICurrentPlaylistView>
    {
        private readonly ICurrentPlaylistService currentPlaylistService;

        private readonly ISongsService songsService;

        public CurrentPlaylistViewPresenter(
            IDependencyResolverContainer container, 
            ICurrentPlaylistView view,
            ICurrentPlaylistService currentPlaylistService,
            ISongsService songsService)
            : base(container, view)
        {
            this.currentPlaylistService = currentPlaylistService;
            this.songsService = songsService;
            this.BindingModel = new CurrentPlaylistBindingModel();

            this.currentPlaylistService.PlaylistChanged += (sender, args) => this.UpdateSongs();
            this.UpdateSongs(); 

            this.BindingModel.PlaySelectedSong = new DelegateCommand(this.PlaySelectedSong);
            this.BindingModel.RemoveSelectedSong = new DelegateCommand(this.RemoveSelectedSong);
            this.AddToPlaylistCommand = new DelegateCommand(this.AddToPlaylist);
        }

        public CurrentPlaylistBindingModel BindingModel { get; private set; }

        public DelegateCommand AddToPlaylistCommand { get; private set; }

        public void AddSelectedSongToPlaylist(MusicPlaylist playlist)
        {
            var song = this.BindingModel.Songs[this.View.SelectedSongIndex];
            this.songsService.AddSongToPlaylistAsync(playlist, song);
        }

        private void RemoveSelectedSong()
        {
            var selectedSongIndex = this.View.SelectedSongIndex;
            if (selectedSongIndex >= 0)
            {
                this.currentPlaylistService.RemoveAsync(selectedSongIndex)
                    .ContinueWith(
                        (t) =>
                        {
                            if (selectedSongIndex < this.BindingModel.Songs.Count)
                            {
                                this.View.SelectedSongIndex = selectedSongIndex;
                            }
                            else if (this.BindingModel.Songs.Count > 0)
                            {
                                this.View.SelectedSongIndex = selectedSongIndex - 1;
                            }
                        }, 
                        TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void PlaySelectedSong()
        {
            var selectedSongIndex = this.View.SelectedSongIndex;
            if (selectedSongIndex >= 0)
            {
                this.currentPlaylistService.PlayAsync(selectedSongIndex);
            }
        }

        private void UpdateSongs()
        {
            this.BindingModel.Songs.Clear();
            foreach (var song in this.currentPlaylistService.GetPlaylist())
            {
                this.BindingModel.Songs.Add(song);
            }
        }

        private void AddToPlaylist()
        {
            this.songsService.GetAllPlaylistsAsync().ContinueWith(
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