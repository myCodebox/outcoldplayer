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

    public interface IPlaylistsWebService
    {
        Task<GoogleMusicPlaylists> GetAllPlaylistsAsync();

        Task<GoogleMusicPlaylist> GetPlaylistAsync(Guid playlistId);

        Task<List<GoogleMusicSong>> GetAllSongsAsync(IProgress<int> progress = null);

        Task<List<GoogleMusicSong>> StreamingLoadAllTracksAsync(IProgress<int> progress);

        Task<AddPlaylistResp> CreatePlaylistAsync(string name);

        Task<bool> DeletePlaylistAsync(Guid id);

        Task<bool> ChangePlaylistNameAsync(Guid id, string name);

        Task<AddSongResp> AddSongToPlaylistAsync(Guid playlistId, Guid songId);

        Task<bool> RemoveSongFromPlaylistAsync(Guid playlistId, Guid songId, Guid entryId);

        Task<StatusResp> GetStatusAsync();
    }

    public class PlaylistsWebService : IPlaylistsWebService
    {
        private const string PlaylistsUrl = "music/services/loadplaylist";
        private const string AllSongsUrl = "music/services/loadalltracks";
        private const string AddPlaylistUrl = "music/services/addplaylist";
        private const string DeletePlaylistUrl = "music/services/deleteplaylist";
        private const string ChangePlaylistNameUrl = "music/services/modifyplaylist";
        private const string AddToPlaylistUrl = "music/services/addtoplaylist";
        private const string DeleteSongUrl = "music/services/deletesong";
        private const string GetStatusUrl = "music/services/getstatus";

        private const string StreamingLoadAllTracks = "music/services/streamingloadalltracks?json=";

        private readonly IGoogleMusicWebService googleMusicWebService;
        private readonly IGoogleMusicSessionService sessionService;

        private DateTime? lastStreamingRequest;

        public PlaylistsWebService(
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

        public async Task<GoogleMusicPlaylists> GetAllPlaylistsAsync()
        {
            return await this.googleMusicWebService.PostAsync<GoogleMusicPlaylists>(PlaylistsUrl);
        }

        public async Task<GoogleMusicPlaylist> GetPlaylistAsync(Guid playlistId)
        {
            var jsonProperties = new Dictionary<string, string>
                                        {
                                            { 
                                                "id", JsonConvert.ToString(playlistId)
                                            }
                                        };

            return await this.googleMusicWebService.PostAsync<GoogleMusicPlaylist>(PlaylistsUrl, jsonProperties: jsonProperties);
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

        public async Task<AddPlaylistResp> CreatePlaylistAsync(string name)
        {
            var jsonProperties = new Dictionary<string, string>
                                        {
                                            { "title", JsonConvert.ToString(name) },
                                            { "playlistType", JsonConvert.ToString("USER_GENERATED") },
                                            { "songRefs", JsonConvert.SerializeObject(new string[] { }) }
                                        };

            return await this.googleMusicWebService.PostAsync<AddPlaylistResp>(AddPlaylistUrl, jsonProperties: jsonProperties);
        }

        public async Task<bool> DeletePlaylistAsync(Guid id)
        {
            var jsonProperties = new Dictionary<string, string>
                                        {
                                            { "id", JsonConvert.ToString(id) },
                                            { "requestType", JsonConvert.ToString(1) },
                                            { "requestCause", JsonConvert.ToString(1) }
                                        };

            var deletePlaylistResp = await this.googleMusicWebService.PostAsync<DeletePlaylistResp>(DeletePlaylistUrl, jsonProperties: jsonProperties);

            return deletePlaylistResp.DeleteId == id;
        }

        public async Task<bool> ChangePlaylistNameAsync(Guid id, string name)
        {
            var jsonProperties = new Dictionary<string, string>
                                        {
                                            { "playlistId", JsonConvert.ToString(id) },
                                            { "playlistName", JsonConvert.ToString(name) }
                                        };

            var response = await this.googleMusicWebService.PostAsync<CommonResponse>(ChangePlaylistNameUrl, jsonProperties: jsonProperties);
            return !response.Success.HasValue || response.Success.Value;
        }

        public async Task<AddSongResp> AddSongToPlaylistAsync(Guid playlistId, Guid songId)
        {
            var jsonProperties = new Dictionary<string, string>
                                        {
                                            { "playlistId", JsonConvert.ToString(playlistId) },
                                            { "songRefs", JsonConvert.SerializeObject(new[] { new { id = songId, type = 1 } }) }
                                        };

            return await this.googleMusicWebService.PostAsync<AddSongResp>(AddToPlaylistUrl, jsonProperties: jsonProperties);
        }

        public async Task<bool> RemoveSongFromPlaylistAsync(Guid playlistId, Guid songId, Guid entryId)
        {
            var jsonProperties = new Dictionary<string, string>
                                        {
                                            { "listId", JsonConvert.ToString(playlistId) },
                                            { "songIds", JsonConvert.SerializeObject(new[] { songId }) },
                                            { "entryIds", JsonConvert.SerializeObject(new[] { entryId }) }
                                        };

            var response = await this.googleMusicWebService.PostAsync<CommonResponse>(DeleteSongUrl, jsonProperties: jsonProperties);
            return !response.Success.HasValue || response.Success.Value;
        }

        public async Task<StatusResp> GetStatusAsync()
        {
            return await this.googleMusicWebService.PostAsync<StatusResp>(GetStatusUrl, forceJsonBody: false);
        }
    }
}