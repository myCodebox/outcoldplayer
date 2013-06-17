// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web.Models;
    using OutcoldSolutions.GoogleMusic.Web.Synchronization;

    public interface IRadioWebService
    {
        Task<IList<RadioPlaylist>> GetAllAsync();

        Task<IList<Song>> GetRadioSongsAsync(string id, IList<Song> songs = null);

        Task DeleteStationAsync(string id);

        Task RenameStationAsync(RadioPlaylist playlist, string name);

        Task<Tuple<RadioPlaylist, IList<Song>>> CreateStationAsync(Song song);
    }

    public class RadioWebService : IRadioWebService
    {
        private const string GetAllRadios = "/music/services/radio/loadradio?u=0";
        private const string FetchRadioFeed = "/music/services/radio/fetchradiofeed?u=0";
        private const string DeleteStation = "/music/services/radio/deletestation";
        private const string RenameStation = "/music/services/radio/renamestation";
        private const string CreateStation = "/music/services/radio/createstation?u=0";

        private readonly IGoogleMusicWebService webService;

        private readonly ISongsRepository songsRepository;

        public RadioWebService(
            IGoogleMusicWebService webService,
            ISongsRepository songsRepository)
        {
            this.webService = webService;
            this.songsRepository = songsRepository;
        }

        public async Task<IList<RadioPlaylist>> GetAllAsync()
        {
            List<RadioPlaylist> radioPlaylists = new List<RadioPlaylist>();

            var resp = await this.webService.PostAsync<RadioResp>(GetAllRadios);

            if (resp != null && resp.MyStation != null)
            {
                foreach (var googleRadio in resp.MyStation)
                {
                    var radioPlaylist = this.ConvertToPlaylist(googleRadio);
                    radioPlaylists.Add(radioPlaylist);
                }
            }

            return radioPlaylists;
        }

        public async Task<IList<Song>> GetRadioSongsAsync(string id, IList<Song> radioSongs = null)
        {
            var jsonProperties = new Dictionary<string, string> { { "id", JsonConvert.ToString(id) } };

            if (radioSongs != null)
            {
                // jsonProperties.Add("radioSeedId", JsonConvert.SerializeObject(radioSongs.Select(x => new { seedId = x.ProviderSongId, seedType = x.IsLibrary ? "TRACK_LOCKER_ID" : "TRACK_MATCHED_ID" }).ToArray()));
            }

            var radio = await this.webService.PostAsync<FetchRadioFeedResp>(FetchRadioFeed, jsonProperties: jsonProperties);

            IList<Song> songs = null;

            if (radio != null && radio.Track != null)
            {
                songs = await this.GetSongsAsync(radio);
            }

            return songs ?? new List<Song>();
        }

        public async Task DeleteStationAsync(string id)
        {
            var jsonProperties = new Dictionary<string, string> { { "id", JsonConvert.ToString(id) } };
            await this.webService.PostAsync<CommonResponse>(DeleteStation, jsonProperties: jsonProperties);
        }

        public async Task RenameStationAsync(RadioPlaylist playlist, string name)
        {
            var jsonProperties = new Dictionary<string, string>
                                     {
                                         { "id", JsonConvert.ToString(playlist.Id) },
                                         { "name", JsonConvert.ToString(name) },
                                         { "radioSeedId", JsonConvert.SerializeObject(new { seedType = playlist.SeedType, seedId = playlist.SeedId }) }
                                     };

            await this.webService.PostAsync<CommonResponse>(RenameStation, jsonProperties: jsonProperties);
        }

        public async Task<Tuple<RadioPlaylist, IList<Song>>> CreateStationAsync(Song song)
        {
            var seedType = song.IsLibrary ? "TRACK_LOCKER_ID" : "TRACK_MATCHED_ID";
            var seedId = song.ProviderSongId;

            var jsonProperties = new Dictionary<string, string>
                                     {
                                         { "seedId", JsonConvert.ToString(seedId) },
                                         { "seedType", JsonConvert.ToString(seedType) },
                                         { "name", JsonConvert.ToString(song.Title) }
                                     };

            var radioResp = await this.webService.PostAsync<FetchRadioFeedResp>(CreateStation, jsonProperties: jsonProperties);

            if (radioResp.Success.HasValue && !radioResp.Success.Value)
            {
                return null;
            }

            RadioPlaylist playlist = new RadioPlaylist()
                                         {
                                             Id = radioResp.Id,
                                             Title = song.Title,
                                             TitleNorm = song.Title.Normalize(),
                                             SeedId = seedId,
                                             SeedType = seedType
                                         };

            return Tuple.Create(playlist, await this.GetSongsAsync(radioResp));
        }

        private RadioPlaylist ConvertToPlaylist(GoogleRadio googleRadio)
        {
            var radioPlaylist = new RadioPlaylist()
            {
                Id = googleRadio.Id,
                Title = googleRadio.Name,
                TitleNorm = googleRadio.Name.Normalize(),
                LastPlayed = DateTimeExtensions.FromUnixFileTime(googleRadio.RecentTimestamp / 1000)
            };

            if (googleRadio.ImageUrl != null && googleRadio.ImageUrl.Length > 0)
            {
                radioPlaylist.ArtUrl = new Uri(googleRadio.ImageUrl[0]);
            }

            if (googleRadio.RadioSeedId != null)
            {
                radioPlaylist.SeedId = googleRadio.RadioSeedId.SeedId;
                radioPlaylist.SeedType = googleRadio.RadioSeedId.SeedType;
            }

            return radioPlaylist;
        }

        private async Task<IList<Song>> GetSongsAsync(FetchRadioFeedResp radio)
        {
            List<Song> songs = new List<Song>();

            foreach (var track in radio.Track)
            {
                Song song = null;
                if (!string.Equals(track.Type, "EPHEMERAL_SUBSCRIPTION", StringComparison.OrdinalIgnoreCase))
                {
                    song = await this.songsRepository.FindSongAsync(track.Id);
                }

                if (song == null)
                {
                    song = track.ToSong();
                    song.IsLibrary = false;
                }

                songs.Add(song);
            }

            return songs;
        }
    }
}