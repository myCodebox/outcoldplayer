// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface IAllAccessWebService
    {
        Task<GoogleMusicArtist> FetchArtistAsync(string googleArtistId, CancellationToken cancellationToken);

        Task<GoogleMusicAlbum> FetchAlbumAsync(string googleAlbumId, CancellationToken cancellationToken);

        Task<GoogleSearchResult> SearchAsync(string query, CancellationToken cancellationToken);

        Task<GoogleMusicGenres> FetchGenresAsync(AllAccessGenre parent, CancellationToken cancellationToken);

        // 0 = recommended, 1 = featured, 2 = new releases 
        Task<GoogleMusicTabs> FetchTabAsync(AllAccessGenre parent, int tab, CancellationToken cancellationToken);

        Task<GoogleMusicSituations> FetchSituationsAsync(CancellationToken cancellationToken);
    }

    public class AllAccessWebService : IAllAccessWebService
    {
        private const string FetchArtist = "fetchartist?nid={0}&include-albums=true&num-top-tracks=20&num-related-artists=20";
        private const string FetchAlbum = "fetchalbum?nid={0}&include-tracks=true";
        private const string Search = "query?q={0}&max-results=20";
        private const string ExploreGenres = "explore/genres";
        private const string ExploreTabs = "explore/tabs?num-items=25&tabs=";
        private const string Situations = "listennow/situations";

        private readonly IGoogleMusicApisService googleMusicApisService;

        private readonly ILogger logger;

        public AllAccessWebService(
            IGoogleMusicApisService googleMusicApisService,
            ILogManager logManager)
        {
            this.googleMusicApisService = googleMusicApisService;
            this.logger = logManager.CreateLogger("SongsWebService");
        }

        public Task<GoogleMusicArtist> FetchArtistAsync(string googleArtistId, CancellationToken cancellationToken)
        {
            return this.googleMusicApisService.GetAsync<GoogleMusicArtist>(string.Format(CultureInfo.InvariantCulture, FetchArtist, googleArtistId), cancellationToken, useCache: true);
        }

        public Task<GoogleMusicAlbum> FetchAlbumAsync(string googleAlbumId, CancellationToken cancellationToken)
        {
            return this.googleMusicApisService.GetAsync<GoogleMusicAlbum>(string.Format(CultureInfo.InvariantCulture, FetchAlbum, googleAlbumId), cancellationToken, useCache: true);
        }

        public Task<GoogleSearchResult> SearchAsync(string query, CancellationToken cancellationToken)
        {
            return this.googleMusicApisService.GetAsync<GoogleSearchResult>(string.Format(CultureInfo.InvariantCulture, Search, WebUtility.UrlEncode(query)), cancellationToken, useCache: true);
        }

        public Task<GoogleMusicGenres> FetchGenresAsync(AllAccessGenre parent, CancellationToken cancellationToken)
        {
            return this.googleMusicApisService.GetAsync<GoogleMusicGenres>(ExploreGenres + (parent == null ? string.Empty : ("?parent-genre=" + parent.Id)), cancellationToken, useCache: true);
        }

        public Task<GoogleMusicTabs> FetchTabAsync(AllAccessGenre parent, int tab, CancellationToken cancellationToken)
        {
            return this.googleMusicApisService.GetAsync<GoogleMusicTabs>(ExploreTabs + tab + (parent == null ? string.Empty : ("&genre=" + parent.Id)), cancellationToken, useCache: true);
        }

        public Task<GoogleMusicSituations> FetchSituationsAsync(CancellationToken cancellationToken)
        {
            return this.googleMusicApisService.PostAsync<GoogleMusicSituations>(
                Situations, 
                json: new
                {
                    requestSignals = new
                    {
                        timeZoneOffsetSecs = TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds
                    }
                },
                cancellationToken: cancellationToken,
                useCache: true);
        }
    }
}
