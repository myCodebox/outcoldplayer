// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public interface ICurrentPlaylistService
    {
        void ClearPlaylist();

        void AddSongs(IEnumerable<GoogleMusicSong> songs);

        void PlaySongs(IEnumerable<GoogleMusicSong> songs);
    }
}