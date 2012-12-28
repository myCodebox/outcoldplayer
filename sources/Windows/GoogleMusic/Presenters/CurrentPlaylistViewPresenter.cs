// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
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
            var songBindingModel = this.View.SelectedSong;
            if (songBindingModel != null)
            {
                this.currentPlaylistService.Remove(songBindingModel.GetSong());
            }
        }

        private void PlaySelectedSong()
        {
            var songBindingModel = this.View.SelectedSong;
            if (songBindingModel != null)
            {
                this.currentPlaylistService.Play(songBindingModel.GetSong());
            }
        }

        private void UpdateSongs()
        {
            this.BindingModel.Songs.Clear();
            foreach (var googleMusicSong in this.currentPlaylistService.GetPlaylist())
            {
                this.BindingModel.Songs.Add(new SongBindingModel(googleMusicSong));
            }
        }
    }
}