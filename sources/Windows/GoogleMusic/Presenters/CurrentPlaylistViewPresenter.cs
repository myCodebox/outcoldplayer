// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class CurrentPlaylistViewPresenter : ViewPresenterBase<ICurrentPlaylistView>
    {
        private readonly ICurrentPlaylistService currentPlaylistService;

        public CurrentPlaylistViewPresenter(
            IDependencyResolverContainer container, 
            ICurrentPlaylistView view,
            ICurrentPlaylistService currentPlaylistService)
            : base(container, view)
        {
            this.currentPlaylistService = currentPlaylistService;
            this.BindingModel = new CurrentPlaylistBindingModel();

            this.currentPlaylistService.PlaylistChanged += (sender, args) => this.UpdateSongs();
            this.UpdateSongs(); 

            this.BindingModel.PlaySelectedSong = new DelegateCommand(this.PlaySelectedSong);
            this.BindingModel.RemoveSelectedSong = new DelegateCommand(this.RemoveSelectedSong);
        }

        public CurrentPlaylistBindingModel BindingModel { get; private set; }

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
    }
}