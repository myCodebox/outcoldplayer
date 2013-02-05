// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.Models;

    public abstract class PlaylistsServiceBase<TPlaylist> : IPlaylistsService<TPlaylist>
        where TPlaylist : Playlist
    {
        public int Count()
        {
            return this.GetPlaylists().Count;
        }

        public IEnumerable<TPlaylist> GetAll(Order order = Order.Name)
        {
            return this.OrderCollection(this.GetPlaylists(), order);
        }

        protected abstract List<TPlaylist> GetPlaylists();

        protected IEnumerable<TPlaylist> OrderCollection(IEnumerable<TPlaylist> playlists, Order order)
        {
            if (order == Order.LastPlayed)
            {
                playlists = playlists.OrderByDescending(x => x.Songs.Count > 0 ? x.Songs.Max(s => s.GoogleMusicMetadata.LastPlayed) : double.MinValue);
            }
            else if (order == Order.Name)
            {
                playlists = playlists.OrderBy(x => (x.Title ?? string.Empty).ToUpper());
            }

            return playlists;
        }
    }
}