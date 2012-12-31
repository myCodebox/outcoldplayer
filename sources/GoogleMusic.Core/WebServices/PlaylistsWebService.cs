// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.WebServices
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public interface IPlaylistsWebService
    {
        Task<GoogleMusicPlaylists> GetAllPlaylistsAsync();

        Task<List<GoogleMusicSong>> GetAllSongsAsync();

        Task<AddPlaylistResp> CreatePlaylistAsync(string name);

        Task<bool> DeletePlaylistAsync(string id);

        Task<bool> ChangePlaylistNameAsync(string id, string name);

        Task<AddSongResp> AddSongToPlaylistAsync(string playlistId, string songId);

        Task<bool> RemoveSongFromPlaylistAsync(string playlistId, string songId, string entryId);
    }

    public class PlaylistsWebService : IPlaylistsWebService
    {
        private const string PlaylistsUrl = "https://play.google.com/music/services/loadplaylist";
        private const string AllSongsUrl = "https://play.google.com/music/services/loadalltracks";
        private const string AddPlaylistUrl = "https://play.google.com/music/services/addplaylist";
        private const string DeletePlaylistUrl = "https://play.google.com/music/services/deleteplaylist";
        private const string ChangePlaylistNameUrl = "https://play.google.com/music/services/modifyplaylist";
        private const string AddToPlaylistUrl = "https://play.google.com/music/services/addtoplaylist";
        private const string DeleteSongUrl = "https://play.google.com/music/services/deletesong";

        private readonly IGoogleWebService googleWebService;
        private readonly IUserDataStorage userDataStorage;

        public PlaylistsWebService(
            IGoogleWebService googleWebService,
            IUserDataStorage userDataStorage)
        {
            this.googleWebService = googleWebService;
            this.userDataStorage = userDataStorage;
        }

        public async Task<GoogleMusicPlaylists> GetAllPlaylistsAsync()
        {
            var requestParameters = new Dictionary<string, string>
                                        {
                                            { "json", JsonConvert.SerializeObject(new { sessionId = this.userDataStorage.GetUserSession().SessionId }) }
                                        };

            var response = await this.googleWebService.PostAsync(PlaylistsUrl, arguments: requestParameters);

            return response.GetAsJsonObject<GoogleMusicPlaylists>();
        }

        public async Task<List<GoogleMusicSong>> GetAllSongsAsync()
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

                var response = await this.googleWebService.PostAsync(AllSongsUrl, arguments: requestParameters);
                playlist = response.GetAsJsonObject<GoogleMusicPlaylist>();
                if (playlist != null && playlist.Playlist != null)
                {
                    googleMusicSongs.AddRange(playlist.Playlist);
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

            var response = await this.googleWebService.PostAsync(AddPlaylistUrl, arguments: requestParameters);

            if (response.HttpWebResponse.StatusCode == HttpStatusCode.OK)
            {
                return response.GetAsJsonObject<AddPlaylistResp>();
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

            var response = await this.googleWebService.PostAsync(DeletePlaylistUrl, arguments: requestParameters);

            return response.HttpWebResponse.StatusCode == HttpStatusCode.OK
                   && string.Equals(response.GetAsJsonObject<DeletePlaylistResp>().DeleteId, id, StringComparison.OrdinalIgnoreCase);
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

            var response = await this.googleWebService.PostAsync(ChangePlaylistNameUrl, arguments: requestParameters);

            return response.HttpWebResponse.StatusCode == HttpStatusCode.OK;
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

            var response = await this.googleWebService.PostAsync(AddToPlaylistUrl, arguments: requestParameters);

            if (response.HttpWebResponse.StatusCode == HttpStatusCode.OK)
            {
                return response.GetAsJsonObject<AddSongResp>();
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

            var response = await this.googleWebService.PostAsync(DeleteSongUrl, arguments: requestParameters);

            return response.HttpWebResponse.StatusCode == HttpStatusCode.OK;
        }
    }
}