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
    }

    public class PlaylistsWebService : IPlaylistsWebService
    {
        private const string PlaylistsUrl = "https://play.google.com/music/services/loadplaylist";

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
    }
}