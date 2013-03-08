// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface IPlaylistsWebService
    {
        Task<GoogleMusicPlaylists> GetAllAsync();

        Task<GoogleMusicPlaylist> GetAsync(Guid playlistId);
        
        Task<AddPlaylistResp> CreateAsync(string name);

        Task<bool> DeleteAsync(Guid id);

        Task<bool> ChangeNameAsync(Guid id, string name);

        Task<AddSongResp> AddSongAsync(Guid playlistId, IEnumerable<Guid> songIds);

        Task<bool> RemoveSongAsync(Guid playlistId, Guid songId, string entryId);
    }
}