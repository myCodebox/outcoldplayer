// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface IRadioWebService
    {
        Task<IList<GoogleMusicRadio>> GetAllAsync(DateTime? lastUpdate = null, IProgress<int> progress = null, Func<IList<GoogleMusicRadio>, Task> chunkHandler = null);

        Task<GoogleMusicStationFeed> GetRadioSongsAsync(string id, IList<Song> songs = null);

        Task<GoogleMusicMutateResponse> DeleteStationsAsync(IEnumerable<string> ids);

        Task<GoogleMusicMutateResponse> CreateStationAsync(Song song);
    }

    public class RadioWebService : IRadioWebService
    {
        private const string GetAllRadios = "radio/station";
        private const string FetchRadioFeed = "radio/stationfeed";
        private const string EditStation = "radio/editstation";

        private readonly IGoogleMusicApisService googleMusicApisService;
        private readonly ISettingsService settingsService;


        public class RequestCreateMutation
        {
            [JsonProperty("create")]
            public dynamic Create { get; set; }

            [JsonProperty("includeFeed")]
            public bool IncludeFeed { get; set; }

            [JsonProperty("numEntries")]
            public int NumEntries { get; set; }

            [JsonProperty("params")]
            public dynamic Params { get; set; }
        }

        public RadioWebService(
            IGoogleMusicApisService googleMusicApisService,
            ISettingsService settingsService)
        {
            this.googleMusicApisService = googleMusicApisService;
            this.settingsService = settingsService;
        }

        public Task<IList<GoogleMusicRadio>> GetAllAsync(DateTime? lastUpdate = null, IProgress<int> progress = null, Func<IList<GoogleMusicRadio>, Task> chunkHandler = null)
        {
            return this.googleMusicApisService.DownloadList(GetAllRadios, lastUpdate, progress, chunkHandler);
        }

        public Task<GoogleMusicStationFeed> GetRadioSongsAsync(string id, IList<Song> radioSongs = null)
        {
            dynamic jsonStation;

            if (string.IsNullOrEmpty(id))
            {
                jsonStation = new
                            {
                                numEntries = 25,
                                recentlyPlayed = radioSongs.Select(x => new { id = x.SongId, type = (int)x.TrackType }).ToArray(),
                                seed = new
                                       {
                                           seedType = 6
                                       }
                            };
            }
            else
            {
                jsonStation = new
                            {
                                numEntries = 25,
                                recentlyPlayed = radioSongs.Select(x => new { id = x.SongId, type = (int)x.TrackType }).ToArray(),
                                radioId = id
                            };
            }

            return this.googleMusicApisService.PostAsync<GoogleMusicStationFeed>(
                FetchRadioFeed,
                new
                {
                    contentFilter = this.settingsService.GetBlockExplicitSongsInRadio() ? 2 : 1,
                    stations =
                        new[]
                        {
                            jsonStation
                        }
                });
        }

        public Task<GoogleMusicMutateResponse> DeleteStationsAsync(IEnumerable<string> ids)
        {
            return this.googleMusicApisService.PostAsync<GoogleMusicMutateResponse>(EditStation, new
                                                                                       {
                                                                                           mutations = ids.Select(id => new
                                                                                                           {
                                                                                                              delete = id,
                                                                                                              includeFeed = false, 
                                                                                                              numEntries = 0 
                                                                                                           }).ToArray()
                                                                                       });
        }

        public async Task<GoogleMusicMutateResponse> CreateStationAsync(Song song)
        {

            dynamic createMutation;

            if (song.TrackType == StreamType.EphemeralSubscription)
            {
                createMutation = new 
                                {
                                    clientId = Guid.NewGuid().ToString(),
                                    deleted = false,
                                    imageType = 1, // TODO: ?
                                    lastModifiedTimestamp = "-1",
                                    name = song.Title,
                                    recentTimestamp = ((long)song.Recent.ToUnixFileTime() * 1000L).ToString("G", CultureInfo.InvariantCulture),
                                    seed = new { seedType = 2, trackId = song.SongId },
                                    tracks = new object[0]
                                };
            }
            else
            {
                createMutation = new 
                                {
                                    clientId = Guid.NewGuid().ToString(),
                                    deleted = false,
                                    imageType = 1, // TODO: ?
                                    imageUrl = song.AlbumArtUrl,
                                    lastModifiedTimestamp = "-1",
                                    name = song.Title,
                                    recentTimestamp = ((long)song.Recent.ToUnixFileTime() * 1000L).ToString("G", CultureInfo.InvariantCulture),
                                    seed = new { seedType = 1, trackLockerId = song.SongId },
                                    tracks = new object[0],
                                };
            }

            var mutation = new RequestCreateMutation()
            {
                Create = createMutation,
                IncludeFeed = true,
                NumEntries = 25,
                Params =
                    new
                    {
                        contentFilter = this.settingsService.GetBlockExplicitSongsInRadio() ? 2 : 1
                    }
            };

            return await this.googleMusicApisService.PostAsync<GoogleMusicMutateResponse>(
                EditStation,
                new { mutations = new[] { mutation } });
        }
    }
}