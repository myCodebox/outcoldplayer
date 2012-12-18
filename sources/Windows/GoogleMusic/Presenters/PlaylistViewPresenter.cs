// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

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
        }

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

            var playlist = parameter as GoogleMusicPlaylist;
            if (playlist != null && playlist.Playlist != null)
            {
                this.BindingModel = new PlaylistViewBindingModel(playlist);

                // TODO: Temporary solution
                if (playlist.Playlist != null)
                {
                    this.currentPlaylistService.PlaySongs(playlist.Playlist);
                }
            }
            else
            {
                this.BindingModel = null;
                this.Logger.Error("OnNavigatedTo: Playlist it null.");
            }
        }
    }
}