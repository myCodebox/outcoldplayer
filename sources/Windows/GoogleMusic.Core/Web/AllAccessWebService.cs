// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web
{
    using System.Globalization;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface IAllAccessWebService
    {
        Task<GoogleMusicArtist> FetchArtistAsync(string googleArtistId);

        Task<GoogleMusicAlbum> FetchAlbumAsync(string googleAlbumId);
    }

    public class AllAccessWebService : IAllAccessWebService
    {
        private const string FetchArtist = "fetchartist?nid={0}&include-albums=true&num-top-tracks=20&num-related-artists=20";
        private const string FetchAlbum = "fetchalbum?nid={0}&include-tracks=true";

        private readonly IGoogleMusicApisService googleMusicApisService;

        private readonly ILogger logger;

        public AllAccessWebService(
            IGoogleMusicApisService googleMusicApisService,
            ILogManager logManager)
        {
            this.googleMusicApisService = googleMusicApisService;
            this.logger = logManager.CreateLogger("SongsWebService");
        }

        public Task<GoogleMusicArtist> FetchArtistAsync(string googleArtistId)
        {
            return this.googleMusicApisService.GetAsync<GoogleMusicArtist>(string.Format(CultureInfo.InvariantCulture, FetchArtist, googleArtistId));
        }

        public Task<GoogleMusicAlbum> FetchAlbumAsync(string googleAlbumId)
        {
            return this.googleMusicApisService.GetAsync<GoogleMusicAlbum>(string.Format(CultureInfo.InvariantCulture, FetchAlbum, googleAlbumId));
        }
    }
}
