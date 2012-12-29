// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public interface IPlaylistsWebService
    {
        Task<GoogleMusicPlaylists> GetAllPlaylistsAsync();

        Task<List<GoogleMusicSong>> GetAllSongsAsync();
    }

    public class PlaylistsWebService : IPlaylistsWebService
    {
        private const string PlaylistsUrl = "https://play.google.com/music/services/loadplaylist";
        private const string AllSongsUrl = "https://play.google.com/music/services/loadalltracks";

        private readonly IGoogleWebService googleWebService;

        public PlaylistsWebService(IGoogleWebService googleWebService)
        {
            this.googleWebService = googleWebService;
        }

        public async Task<GoogleMusicPlaylists> GetAllPlaylistsAsync()
        {
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { "json", "{}" }
                                        };

            var response = await this.googleWebService.PostAsync(PlaylistsUrl, arguments: requestParameters);

            return response.GetAsJsonObject<GoogleMusicPlaylists>();
        }

        public async Task<List<GoogleMusicSong>> GetAllSongsAsync()
        {
            List<GoogleMusicSong> googleMusicSongs = new List<GoogleMusicSong>();
            
            GoogleMusicPlaylist playlist = null;
            do
            {
                string json = null;

                if (playlist != null && !string.IsNullOrEmpty(playlist.ContinuationToken))
                {
                    json = string.Format("{{continuationToken:\"{0}\"}}", playlist.ContinuationToken);
                }
                else
                {
                    json = "{}";
                }

                var requestParameters = new Dictionary<string, string>
                                        {
                                            { "json", json }
                                        };

                var response = await this.googleWebService.PostAsync(AllSongsUrl, arguments: requestParameters);
                playlist = response.GetAsJsonObject<GoogleMusicPlaylist>();
                if (playlist != null && playlist.Playlist != null)
                {
                    googleMusicSongs.AddRange(playlist.Playlist);
                }
            }
            while (playlist != null && !string.IsNullOrEmpty(playlist.ContinuationToken));

            return googleMusicSongs;
        }
    }
}