// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    public interface ISongsRepository
    {
        Task<IList<SongBindingModel>> GetAllAsync();

        Task<SongBindingModel> GetSongAsync(string songId);

        Task<IList<Song>> GetArtistSongsAsync(Artist artist);

        Task<IList<Song>> GetAlbumSongsAsync(Album album);

        Task<IList<Song>> GetGenreSongsAsync(Genre genre);

        Task<IList<Song>> GetUserPlaylistSongsAsync(UserPlaylist userPlaylist);
    }
}