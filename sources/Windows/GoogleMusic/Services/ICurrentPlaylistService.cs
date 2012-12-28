// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public interface ICurrentPlaylistService
    {
        event EventHandler PlaylistChanged;

        void ClearPlaylist();

        void AddSongs(IEnumerable<GoogleMusicSong> songs);

        IEnumerable<GoogleMusicSong> GetPlaylist();

        Task PlayAsync(int index);

        Task RemoveAsync(int index);
    }
}