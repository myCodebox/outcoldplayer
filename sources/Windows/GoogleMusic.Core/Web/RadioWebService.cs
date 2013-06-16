// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web.Models;
    using OutcoldSolutions.GoogleMusic.Web.Synchronization;

    public interface IRadioWebService
    {
        Task<IList<RadioPlaylist>> GetAllAsync();

        Task<IList<Song>> GetRadioSongsAsync(string id);

        Task DeleteStationAsync(string id);

        Task RenameStationAsync(string id, string name);
    }

    public class RadioWebService : IRadioWebService
    {
        private const string GetAllRadios = "/music/services/radio/loadradio?u=0";
        private const string FetchRadioFeed = "/music/services/radio/fetchradiofeed?u=0";
        private const string DeleteStation = "/music/services/radio/deletestation";
        private const string RenameStation = "/music/services/radio/renamestation";

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

                    radioPlaylists.Add(radioPlaylist);
                }
            }

            return radioPlaylists;
        }

        public async Task<IList<Song>> GetRadioSongsAsync(string id)
        {
            List<Song> songs = new List<Song>();

            var jsonProperties = new Dictionary<string, string> { { "id", JsonConvert.ToString(id) } };
            var radio = await this.webService.PostAsync<FetchRadioFeedResp>(FetchRadioFeed, jsonProperties: jsonProperties);

            if (radio != null && radio.Track != null)
            {
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
            }

            return songs;
        }

        public async Task DeleteStationAsync(string id)
        {
            var jsonProperties = new Dictionary<string, string> { { "id", JsonConvert.ToString(id) } };
            await this.webService.PostAsync<CommonResponse>(DeleteStation, jsonProperties: jsonProperties);
        }

        public async Task RenameStationAsync(string id, string name)
        {
            var jsonProperties = new Dictionary<string, string>
                                     {
                                         { "id", JsonConvert.ToString(id) },
                                         { "name", JsonConvert.ToString(name) }
                                     };

            await this.webService.PostAsync<CommonResponse>(RenameStation, jsonProperties: jsonProperties);
        }
    }
}