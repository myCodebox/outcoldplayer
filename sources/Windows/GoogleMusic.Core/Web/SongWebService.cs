// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web.Models;
    using OutcoldSolutions.Web;

    public interface ISongWebService
    {
        Task<StatusResp> GetStatusAsync();

        Task<List<GoogleMusicSong>> StreamingLoadAllTracksAsync(DateTime? lastUpdate, IProgress<int> progress);

        Task<GoogleMusicSongUrl> GetSongUrlAsync(string id);

        Task<bool> RecordPlayingAsync(string songId, string playlistId, bool updateRecentAlbum, bool updateRecentPlaylist, int playCount);

        Task<RatingResp> UpdateRatingAsync(string songId, int rating);
    }

    public class SongWebService : ISongWebService
    {
        private const string SongUrlFormat = "play?u=0&songid={0}";
        private const string RecordPlayingUrl = "services/recordplaying";
        private const string ModifyEntriesUrl = "services/modifyentries";
        
        private const string GetStatusUrl = "services/getstatus";

        private const string StreamingLoadAllTracks = "services/streamingloadalltracks?json=";

        private readonly IGoogleMusicWebService googleMusicWebService;
        private readonly IGoogleMusicSessionService sessionService;

        public SongWebService(
            IGoogleMusicWebService googleMusicWebService,
            IGoogleMusicSessionService sessionService)
        {
            this.googleMusicWebService = googleMusicWebService;
            this.sessionService = sessionService;
        }

        public async Task<StatusResp> GetStatusAsync()
        {
            return await this.googleMusicWebService.PostAsync<StatusResp>(GetStatusUrl, forceJsonBody: false);
        }
        
        public async Task<List<GoogleMusicSong>> StreamingLoadAllTracksAsync(DateTime? lastUpdate, IProgress<int> progress)
        {
            List<GoogleMusicSong> googleMusicSongs = new List<GoogleMusicSong>();

            string json;

            if (lastUpdate == null)
            {
                json = JsonConvert.SerializeObject(
                        new
                        {
                            sessionId = this.sessionService.GetSession().SessionId,
                            requestCause = 3,
                            requestType = 1
                        });
            }
            else
            {
                json = JsonConvert.SerializeObject(
                        new
                        {
                            sessionId = this.sessionService.GetSession().SessionId,
                            requestCause = 2,
                            requestType = 1,
                            lastUpdated = (long)((lastUpdate.Value - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds * 1000)
                        });
            }

            var response = await this.googleMusicWebService.GetAsync(StreamingLoadAllTracks + WebUtility.UrlEncode(json));

            if (response.Content.IsPlainText())
            {
                var oResponse = await response.Content.ReadAsJsonObject<CommonResponse>();
                if (oResponse.ReloadXsrf.HasValue && oResponse.ReloadXsrf.Value)
                {
                    await this.googleMusicWebService.RefreshXtAsync();
                    response = await this.googleMusicWebService.GetAsync(StreamingLoadAllTracks + WebUtility.UrlEncode(json));
                }
            }

            if (response.Content.IsHtmlText())
            {
                var stream = await response.Content.ReadAsStreamAsync();
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var line = streamReader.ReadLine();
                        if (line == null)
                        {
                            break;
                        }

                        if (line.StartsWith("window.parent['slat_process']("))
                        {
                            string str = line.Substring("window.parent['slat_process'](".Length, line.Length - ("window.parent['slat_process'](".Length + 2));
                            var googleMusicPlaylist = JsonConvert.DeserializeObject<GoogleMusicPlaylist>(str);
                            googleMusicSongs.AddRange(googleMusicPlaylist.Playlist);

                            if (progress != null)
                            {
                                progress.Report(googleMusicSongs.Count);
                            }
                        }
                    }
                }
            }

            return googleMusicSongs;
        }

        public async Task<GoogleMusicSongUrl> GetSongUrlAsync(string id)
        {
            var url = string.Format(SongUrlFormat, id);
            return await this.googleMusicWebService.GetAsync<GoogleMusicSongUrl>(url, signUrl: false);
        }

        public async Task<bool> RecordPlayingAsync(string songId, string playlistId, bool updateRecentAlbum, bool updateRecentPlaylist, int playCount)
        {
            var jsonProperties = new Dictionary<string, string>
                                     {
                                         { "songId", JsonConvert.ToString(songId) },
                                         { "playCount", JsonConvert.ToString(playCount) },
                                         { "updateRecentAlbum", JsonConvert.ToString(updateRecentAlbum) },
                                         { "updateRecentPlaylist", JsonConvert.ToString(updateRecentPlaylist) },
                                         { "playlistId", JsonConvert.ToString(playlistId) }
                                     };

            var response = await this.googleMusicWebService.PostAsync<CommonResponse>(RecordPlayingUrl, jsonProperties: jsonProperties);

            return response.Success.HasValue && response.Success.Value;
        }

        public async Task<RatingResp> UpdateRatingAsync(string songId, int rating)
        {
            var jsonProperties = new Dictionary<string, string>
                                     {
                                         {
                                             "entries", JsonConvert.SerializeObject(new[] { new { id = songId, rating = rating } })
                                         },
                                     };

            return await this.googleMusicWebService.PostAsync<RatingResp>(ModifyEntriesUrl, jsonProperties: jsonProperties);
        }
    }
}