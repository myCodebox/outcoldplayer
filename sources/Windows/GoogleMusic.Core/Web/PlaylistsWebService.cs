// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
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

        Task<AddPlaylistResp> CreatePlaylistAsync(string name);

        Task<bool> DeletePlaylistAsync(string id);

        Task<bool> ChangePlaylistNameAsync(string id, string name);

        Task<AddSongResp> AddSongToPlaylistAsync(string playlistId, string songId);

        Task<bool> RemoveSongFromPlaylistAsync(string playlistId, string songId, string entryId);

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

        private readonly IGoogleMusicWebService googleMusicWebService;
        private readonly IUserDataStorage userDataStorage;

        public PlaylistsWebService(
            IGoogleMusicWebService googleMusicWebService,
            IUserDataStorage userDataStorage)
        {
            this.googleMusicWebService = googleMusicWebService;
            this.userDataStorage = userDataStorage;
        }

        public async Task<GoogleMusicPlaylists> GetAllPlaylistsAsync()
        {
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { "json", JsonConvert.SerializeObject(new { sessionId = this.userDataStorage.GetUserSession().SessionId }) }
                                        };

            var response = await this.googleMusicWebService.PostAsync(PlaylistsUrl, formData: requestParameters);

            return await response.Content.ReadAsJsonObject<GoogleMusicPlaylists>();
        }

        public async Task<GoogleMusicPlaylist> GetPlaylistAsync(string playlistId)
        {
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { 
                                                "json", JsonConvert.SerializeObject(new
                                                                                      {
                                                                                          sessionId = this.userDataStorage.GetUserSession().SessionId,
                                                                                          id = playlistId
                                                                                      }) 
                                            }
                                        };

            var response = await this.googleMusicWebService.PostAsync(PlaylistsUrl, formData: requestParameters);

            return await response.Content.ReadAsJsonObject<GoogleMusicPlaylist>();
        }

        public async Task<List<GoogleMusicSong>> GetAllSongsAsync(IProgress<int> progress = null)
        {
            List<GoogleMusicSong> googleMusicSongs = new List<GoogleMusicSong>();
            
            GoogleMusicPlaylist playlist = null;
            do
            {
                string json = null;
                
                if (playlist != null && !string.IsNullOrEmpty(playlist.ContinuationToken))
                {
                    json = JsonConvert.SerializeObject(new { sessionId = this.userDataStorage.GetUserSession().SessionId, continuationToken = playlist.ContinuationToken });
                }
                else
                {
                    json = JsonConvert.SerializeObject(new { sessionId = this.userDataStorage.GetUserSession().SessionId });
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

        public async Task<AddPlaylistResp> CreatePlaylistAsync(string name)
        {
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { 
                                                "json", JsonConvert.SerializeObject(new
                                                                                      {
                                                                                          sessionId = this.userDataStorage.GetUserSession().SessionId, 
                                                                                          title = name,
                                                                                          playlistType = "USER_GENERATED",
                                                                                          songRefs = new string[] { }
                                                                                      }) 
                                            }
                                        };

            var response = await this.googleMusicWebService.PostAsync(AddPlaylistUrl, formData: requestParameters);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadAsJsonObject<AddPlaylistResp>();
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
                                                                                          sessionId = this.userDataStorage.GetUserSession().SessionId, 
                                                                                          id, 
                                                                                          requestType = 1, 
                                                                                          requestCause = 1
                                                                                      }) 
                                            }
                                        };

            var response = await this.googleMusicWebService.PostAsync(DeletePlaylistUrl, formData: requestParameters);

            return response.StatusCode == HttpStatusCode.OK
                   && string.Equals((await response.Content.ReadAsJsonObject<DeletePlaylistResp>()).DeleteId, id, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<bool> ChangePlaylistNameAsync(string id, string name)
        {
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { 
                                                "json", JsonConvert.SerializeObject(new
                                                                                      {
                                                                                          sessionId = this.userDataStorage.GetUserSession().SessionId, 
                                                                                          playlistId = id, 
                                                                                          playlistName = name
                                                                                      }) 
                                            }
                                        };

            var response = await this.googleMusicWebService.PostAsync(ChangePlaylistNameUrl, formData: requestParameters);

            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<AddSongResp> AddSongToPlaylistAsync(string playlistId, string songId)
        {
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { 
                                                "json", JsonConvert.SerializeObject(new
                                                                                      {
                                                                                          sessionId = this.userDataStorage.GetUserSession().SessionId,
                                                                                          playlistId,
                                                                                          songRefs = new[] { new { id = songId, type = 1 } }
                                                                                      })
                                            }
                                        };

            var response = await this.googleMusicWebService.PostAsync(AddToPlaylistUrl, formData: requestParameters);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadAsJsonObject<AddSongResp>();
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
                                                                                          sessionId = this.userDataStorage.GetUserSession().SessionId,
                                                                                          listId = playlistId,
                                                                                          songIds = new[] { songId },
                                                                                          entryIds = new[] { entryId } 
                                                                                      })
                                            }
                                        };

            var response = await this.googleMusicWebService.PostAsync(DeleteSongUrl, formData: requestParameters);

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