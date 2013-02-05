// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface IPlaylistsService<out TPlaylist> where TPlaylist : Playlist
    {
        int Count();

        IEnumerable<TPlaylist> GetAll(Order order = Order.Name);
    }

    public interface IAlbumsService : IPlaylistsService<Album>
    {
    }

    public interface IArtistsService : IPlaylistsService<Artist>
    {
    }

    public interface IGenresService : IPlaylistsService<Genre>
    {
    }
}