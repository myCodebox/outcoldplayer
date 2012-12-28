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
            var songBindingModel = this.View.SelectedSong;
            if (songBindingModel != null)
            {
                var selectedIndex = this.BindingModel.Songs.IndexOf(songBindingModel);
                this.currentPlaylistService.RemoveAsync(songBindingModel.Index - 1)
                    .ContinueWith(
                        (t) =>
                        {
                            if (selectedIndex < this.BindingModel.Songs.Count)
                            {
                                this.View.SelectedSong = this.BindingModel.Songs[selectedIndex];
                            }
                            else if (this.BindingModel.Songs.Count > 0)
                            {
                                this.View.SelectedSong = this.BindingModel.Songs[selectedIndex - 1];
                            }
                        }, 
                        TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void PlaySelectedSong()
        {
            var songBindingModel = this.View.SelectedSong;
            if (songBindingModel != null)
            {
                this.currentPlaylistService.PlayAsync(songBindingModel.Index - 1);
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