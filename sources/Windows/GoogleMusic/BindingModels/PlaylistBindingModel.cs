// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;

    public class PlaylistBindingModel : BindingModelBase
    {
        private readonly Playlist playlist;

        public PlaylistBindingModel(Playlist playlist)
        {
            this.playlist = playlist;
            this.PlayCommand = new DelegateCommand(() =>
                {
                    var currentPlaylistService = App.Container.Resolve<ICurrentPlaylistService>();

                    currentPlaylistService.ClearPlaylist();
                    if (playlist.Songs.Count > 0)
                    {
                        currentPlaylistService.SetPlaylist(playlist);
                        currentPlaylistService.PlayAsync();
                    }

                    App.Container.Resolve<INavigationService>().NavigateToView<PlaylistViewResolver>(playlist);
                });
        }

        public DelegateCommand PlayCommand { get; set; }

        public bool IsAlbum
        {
            get
            {
                return this.playlist is Album;
            }
        }

        public Playlist Playlist
        {
            get
            {
                return this.playlist;
            }
        }
    }
}