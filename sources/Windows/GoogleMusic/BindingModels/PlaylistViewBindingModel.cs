// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.ObjectModel;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class PlaylistViewBindingModel : BindingModelBase
    {
        private readonly GoogleMusicPlaylist playlist;

        public PlaylistViewBindingModel(GoogleMusicPlaylist playlist)
        {
            this.playlist = playlist;

            if (this.playlist.Playlist != null)
            {
                this.Songs = new ObservableCollection<SongBindingModel>(this.playlist.Playlist.Select(s => new SongBindingModel(s)));
            }
            else
            {
                this.Songs = new ObservableCollection<SongBindingModel>();
            }
        }

        public ObservableCollection<SongBindingModel> Songs { get; private set; }

        public string Title
        {
            get
            {
                return this.playlist.Title;
            }
        }
    }
}