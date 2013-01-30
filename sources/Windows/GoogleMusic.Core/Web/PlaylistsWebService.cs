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

        Task<GoogleMusicPlaylist> GetPlaylistAsync(string playlistId);

        Task<List<GoogleMusicSong>> GetAllSongsAsync(IProgress<int> progress = null);

        Task<List<GoogleMusicSong>> StreamingLoadAllTracksAsync(IProgress<int> progress);

        Task<AddPlaylistResp> CreatePlaylistAsync(string name);

        Task<bool> DeletePlaylistAsync(string id);

        Task<bool> ChangePlaylistNameAsync(string id, string name);

        Task<AddSongResp> AddSongToPlaylistAsync(string playlistId, string songId);

        Task<bool> RemoveSongFromPlaylistAsync(string playlistId, string songId, string entryId);

        Task<StatusResp> GetStatusAsync();
    }

    public class PlaylistsWebService : WebServiceBase, IPlaylistsWebService
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
            IGoogleMusicSessionService sessionService,
            IGoogleAccountWebService googleAccountWebService,
            IGoogleAccountService googleAccountService)
            : base(googleAccountWebService, googleAccountService, googleMusicWebService, sessionService)
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
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { "json", JsonConvert.SerializeObject(new { sessionId = this.sessionService.GetSession().SessionId }) }
                                        };

            var response = await this.googleMusicWebService.PostAsync(PlaylistsUrl, formData: requestParameters);
            var googleMusicPlaylists = await response.Content.ReadAsJsonObject<GoogleMusicPlaylists>();

            if (await this.NeedRetry(googleMusicPlaylists))
            {
                response = await this.googleMusicWebService.PostAsync(PlaylistsUrl, formData: requestParameters);
                googleMusicPlaylists = await response.Content.ReadAsJsonObject<GoogleMusicPlaylists>();
            }

            return googleMusicPlaylists;
        }

        public async Task<GoogleMusicPlaylist> GetPlaylistAsync(string playlistId)
        {
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { 
                                                "json", JsonConvert.SerializeObject(new
                                                                                      {
                                                                                          sessionId = this.sessionService.GetSession().SessionId,
                                                                                          id = playlistId
                                                                                      }) 
                                            }
                                        };

            var response = await this.googleMusicWebService.PostAsync(PlaylistsUrl, formData: requestParameters);

            var googleMusicPlaylist = await response.Content.ReadAsJsonObject<GoogleMusicPlaylist>();

            if (await this.NeedRetry(googleMusicPlaylist))
            {
                response = await this.googleMusicWebService.PostAsync(PlaylistsUrl, formData: requestParameters);
                googleMusicPlaylist = await response.Content.ReadAsJsonObject<GoogleMusicPlaylist>();
            }

            return googleMusicPlaylist;
        }

        public async Task<List<GoogleMusicSong>> GetAllSongsAsync(IProgress<int> progress = null)
        {
            List<GoogleMusicSong> googleMusicSongs = new List<GoogleMusicSong>();

            this.lastStreamingRequest = DateTime.UtcNow;

            GoogleMusicPlaylist playlist = null;
            do
            {
                string json = null;
                
                if (playlist != null && !string.IsNullOrEmpty(playlist.ContinuationToken))
                {
                    json = JsonConvert.SerializeObject(new { sessionId = this.sessionService.GetSession().SessionId, continuationToken = playlist.ContinuationToken });
                }
                else
                {
                    json = JsonConvert.SerializeObject(new { sessionId = this.sessionService.GetSession().SessionId });
                }

                var requestParameters = new Dictionary<string, string>
                                        {
                                            { "json", json }
                                        };

                var response = await this.googleMusicWebService.PostAsync(AllSongsUrl, formData: requestParameters);
                playlist = await response.Content.ReadAsJsonObject<GoogleMusicPlaylist>();
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
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { 
                                                "json", JsonConvert.SerializeObject(new
                                                                                      {
                                                                                          sessionId = this.sessionService.GetSession().SessionId, 
                                                                                          title = name,
                                                                                          playlistType = "USER_GENERATED",
                                                                                          songRefs = new string[] { }
                                                                                      }) 
                                            }
                                        };

            var response = await this.googleMusicWebService.PostAsync(AddPlaylistUrl, formData: requestParameters);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var addPlaylistResp = await response.Content.ReadAsJsonObject<AddPlaylistResp>();

                if (await this.NeedRetry(addPlaylistResp))
                {
                    response = await this.googleMusicWebService.PostAsync(AddPlaylistUrl, formData: requestParameters);
                    addPlaylistResp = await response.Content.ReadAsJsonObject<AddPlaylistResp>();
                }

                return addPlaylistResp;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> DeletePlaylistAsync(string id)
        {
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { 
                                                "json", JsonConvert.SerializeObject(new
                                                                                      {
                                                                                          sessionId = this.sessionService.GetSession().SessionId, 
                                                                                          id, 
                                                                                          requestType = 1, 
                                                                                          requestCause = 1
                                                                                      }) 
                                            }
                                        };

            var response = await this.googleMusicWebService.PostAsync(DeletePlaylistUrl, formData: requestParameters);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var deletePlaylistResp = await response.Content.ReadAsJsonObject<DeletePlaylistResp>();
                if (await this.NeedRetry(deletePlaylistResp))
                {
                    response = await this.googleMusicWebService.PostAsync(DeletePlaylistUrl, formData: requestParameters);
                    deletePlaylistResp = await response.Content.ReadAsJsonObject<DeletePlaylistResp>();
                }

                return response.StatusCode == HttpStatusCode.OK && string.Equals(deletePlaylistResp.DeleteId, id, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public async Task<bool> ChangePlaylistNameAsync(string id, string name)
        {
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { 
                                                "json", JsonConvert.SerializeObject(new
                                                                                      {
                                                                                          sessionId = this.sessionService.GetSession().SessionId, 
                                                                                          playlistId = id, 
                                                                                          playlistName = name
                                                                                      }) 
                                            }
                                        };

            var response = await this.googleMusicWebService.PostAsync(ChangePlaylistNameUrl, formData: requestParameters);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var objectResponse = await response.Content.ReadAsJsonObject<CommonResponse>();

                if (await this.NeedRetry(objectResponse))
                {
                    response = await this.googleMusicWebService.PostAsync(ChangePlaylistNameUrl, formData: requestParameters);
                }

                return response.IsSuccessStatusCode;
            }
            else
            {
                return false;
            }
        }

        public async Task<AddSongResp> AddSongToPlaylistAsync(string playlistId, string songId)
        {
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { 
                                                "json", JsonConvert.SerializeObject(new
                                                                                      {
                                                                                          sessionId = this.sessionService.GetSession().SessionId,
                                                                                          playlistId,
                                                                                          songRefs = new[] { new { id = songId, type = 1 } }
                                                                                      })
                                            }
                                        };

            var response = await this.googleMusicWebService.PostAsync(AddToPlaylistUrl, formData: requestParameters);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var addSongResp = await response.Content.ReadAsJsonObject<AddSongResp>();

                if (await this.NeedRetry(addSongResp))
                {
                    response = await this.googleMusicWebService.PostAsync(AddToPlaylistUrl, formData: requestParameters);
                    addSongResp = await response.Content.ReadAsJsonObject<AddSongResp>();
                }

                return addSongResp;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> RemoveSongFromPlaylistAsync(string playlistId, string songId, string entryId)
        {
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { 
                                                "json", JsonConvert.SerializeObject(new
                                                                                      {
                                                                                          sessionId = this.sessionService.GetSession().SessionId,
                                                                                          listId = playlistId,
                                                                                          songIds = new[] { songId },
                                                                                          entryIds = new[] { entryId } 
                                                                                      })
                                            }
                                        };

            var response = await this.googleMusicWebService.PostAsync(DeleteSongUrl, formData: requestParameters);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var commonResponse = await response.Content.ReadAsJsonObject<CommonResponse>();

                if (await this.NeedRetry(commonResponse))
                {
                    response = await this.googleMusicWebService.PostAsync(DeleteSongUrl, formData: requestParameters);
                    commonResponse = await response.Content.ReadAsJsonObject<CommonResponse>();
                }
            }

            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<StatusResp> GetStatusAsync()
        {
            var responseMessage = await this.googleMusicWebService.PostAsync(GetStatusUrl);

            if (responseMessage.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    return await responseMessage.Content.ReadAsJsonObject<StatusResp>();
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}