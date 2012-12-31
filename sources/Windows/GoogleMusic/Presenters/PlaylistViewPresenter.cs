// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class PlaylistViewPresenter : ViewPresenterBase<IPlaylistView>
    {
        private readonly ICurrentPlaylistService currentPlaylistService;

        private PlaylistViewBindingModel bindingModel;

        public PlaylistViewPresenter(
            IDependencyResolverContainer container, 
            IPlaylistView view,
            ICurrentPlaylistService currentPlaylistService)
            : base(container, view)
        {
            this.currentPlaylistService = currentPlaylistService;
            this.PlaySelectedSongCommand = new DelegateCommand(this.PlaySelectedSong);
        }

        public DelegateCommand PlaySelectedSongCommand { get; private set; }

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
    }
}