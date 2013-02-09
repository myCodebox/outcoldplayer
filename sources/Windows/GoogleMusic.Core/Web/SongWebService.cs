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

    public class SongWebService : ISongWebService
    {
        private const string SongUrlFormat = "music/play?u=0&songid={0}";
        private const string RecordPlayingUrl = "music/services/recordplaying";
        private const string ModifyEntriesUrl = "music/services/modifyentries";
        private const string AllSongsUrl = "music/services/loadalltracks";
        private const string GetStatusUrl = "music/services/getstatus";

        private const string StreamingLoadAllTracks = "music/services/streamingloadalltracks?json=";

        private readonly IGoogleMusicWebService googleMusicWebService;
        private readonly IGoogleMusicSessionService sessionService;

        private DateTime? lastStreamingRequest;

        public SongWebService(
            IGoogleMusicWebService googleMusicWebService,
            IGoogleMusicSessionService sessionService)
        {
            this.googleMusicWebService = googleMusicWebService;
            this.sessionService = sessionService;

            this.sessionService.SessionCleared += (sender, args) =>
            {
                this.lastStreamingRequest = null;
            };
        }

        public async Task<StatusResp> GetStatusAsync()
        {
            return await this.googleMusicWebService.PostAsync<StatusResp>(GetStatusUrl, forceJsonBody: false);
        }

        public async Task<List<GoogleMusicSong>> GetAllSongsAsync(IProgress<int> progress = null)
        {
            List<GoogleMusicSong> googleMusicSongs = new List<GoogleMusicSong>();

            this.lastStreamingRequest = DateTime.UtcNow;

            GoogleMusicPlaylist playlist = null;
            do
            {
                var jsonProperties = new Dictionary<string, string>();

                if (playlist != null && !string.IsNullOrEmpty(playlist.ContinuationToken))
                {
                    jsonProperties.Add("continuationToken", JsonConvert.ToString(playlist.ContinuationToken));
                }

                playlist = await this.googleMusicWebService.PostAsync<GoogleMusicPlaylist>(AllSongsUrl, jsonProperties: jsonProperties);
                if (playlist != null && playlist.Playlist != null)
                {
                    googleMusicSongs.AddRange(playlist.Playlist);
                }

                if (progress != null)
                {
                    progress.Report(googleMusicSongs.Count);
                }
            }
            while (playlist != null && !string.IsNullOrEmpty(playlist.ContinuationToken));

            return googleMusicSongs;
        }

        public async Task<List<GoogleMusicSong>> StreamingLoadAllTracksAsync(IProgress<int> progress)
        {
            List<GoogleMusicSong> googleMusicSongs = new List<GoogleMusicSong>();

            string json;

            if (this.lastStreamingRequest == null)
            {
                json = JsonConvert.SerializeObject(
                        new
                        {
                            sessionId = this.sessionService.GetSession().SessionId,
                            requestCause = 3,
                            requestType = 1
                        });

                this.lastStreamingRequest = DateTime.UtcNow;
            }
            else
            {
                json = JsonConvert.SerializeObject(
                        new
                        {
                            sessionId = this.sessionService.GetSession().SessionId,
                            requestCause = 2,
                            requestType = 1,
                            lastUpdated = (long)((this.lastStreamingRequest.Value - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds * 1000)
                        });

                this.lastStreamingRequest = DateTime.UtcNow;
            }

            var response = await this.googleMusicWebService.GetAsync(StreamingLoadAllTracks + WebUtility.UrlEncode(json));

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

            return googleMusicSongs;
        }

        public async Task<GoogleMusicSongUrl> GetSongUrlAsync(Guid id)
        {
            var url = string.Format(SongUrlFormat, id);
            return await this.googleMusicWebService.GetAsync<GoogleMusicSongUrl>(url, signUrl: false);
        }

        public async Task<bool> RecordPlayingAsync(Guid songId, string playlistId, bool updateRecentAlbum, bool updateRecentPlaylist, int playCount)
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

        public async Task<RatingResp> UpdateRatingAsync(Guid songId, int rating)
        {
            var jsonProperties = new Dictionary<string, string>
                                     {
                                         { "id", JsonConvert.ToString(songId) },
                                         { "rating", JsonConvert.ToString(rating) }
                                     };

            return await this.googleMusicWebService.PostAsync<RatingResp>(ModifyEntriesUrl, jsonProperties: jsonProperties);
        }
    }
}