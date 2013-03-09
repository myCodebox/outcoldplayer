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

        Task<GoogleMusicPlaylist> GetAsync(string playlistId);
        
        Task<AddPlaylistResp> CreateAsync(string name);

        Task<bool> DeleteAsync(string id);

        Task<bool> ChangeNameAsync(string id, string name);

        Task<AddSongResp> AddSongAsync(string playlistId, IEnumerable<string> songIds);

        Task<bool> RemoveSongAsync(string playlistId, string songId, string entryId);
    }
}