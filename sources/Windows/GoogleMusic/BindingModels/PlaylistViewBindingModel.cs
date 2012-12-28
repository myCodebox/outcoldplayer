// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class PlaylistViewBindingModel : SongsBindingModelBase
    {
        private readonly GoogleMusicPlaylist playlist;

        public PlaylistViewBindingModel(GoogleMusicPlaylist playlist)
        {
            this.playlist = playlist;

            if (this.playlist.Playlist != null)
            {
                foreach (var song in this.playlist.Playlist.Select(s => new SongBindingModel(s)))
                {
                    this.Songs.Add(song);
                }
            }
        }

        public string Title
        {
            get
            {
                return this.playlist.Title;
            }
        }
    }
}