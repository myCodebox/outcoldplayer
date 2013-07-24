// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Text;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web.Models;
    using OutcoldSolutions.Web;

    using Windows.Security.Cryptography;
    using Windows.Security.Cryptography.Core;

    public interface ISongsWebService
    {
        Task<StatusResp> GetStatusAsync();

        Task<List<GoogleMusicSong>> StreamingLoadAllTracksAsync(DateTime? lastUpdate, IProgress<int> progress);

        Task<GoogleMusicSongUrl> GetSongUrlAsync(Song song);

        Task<bool> RecordPlayingAsync(string songId, string playlistId, bool updateRecentAlbum, bool updateRecentPlaylist, int playCount);

        Task<RatingResp> UpdateRatingAsync(string songId, int rating);
    }

    public class SongsWebService : ISongsWebService
    {
        private const string AlpanumLowercase = "abcdefghijklmnopqrstuvwxyz" + "0123456789";
        private const string DefaultGoogleKey = "27f7313e-f75d-445a-ac99-56386a5fe879";

        private const string SettingsGoogleKeyName = "GoogleAAKey";

        private const string SongUrlFormat = "play?songid={0}&pt=e&dt=pe&targetkbps=320&start=0";
        private const string SongUrlFromStoreFormat = "play?mjck={0}&slt={1}&sig={2}&pt=e&dt=pe&targetkbps=320&start=0";
        private const string RecordPlayingUrl = "services/recordplaying";
        private const string ModifyEntriesUrl = "services/modifyentries";
        
        private const string GetStatusUrl = "services/getstatus";

        private const string StreamingLoadAllTracks = "services/streamingloadalltracks?json=";

        private readonly IGoogleMusicWebService googleMusicWebService;
        private readonly IGoogleMusicSessionService sessionService;
        private readonly ISettingsService settingsService;
        private readonly ILogger logger;

        public SongsWebService(
            IGoogleMusicWebService googleMusicWebService,
            IGoogleMusicSessionService sessionService,
            ISettingsService settingsService,
            ILogManager logManager)
        {
            this.googleMusicWebService = googleMusicWebService;
            this.sessionService = sessionService;
            this.settingsService = settingsService;
            this.logger = logManager.CreateLogger("SongsWebService");
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

        public async Task<GoogleMusicSongUrl> GetSongUrlAsync(Song song)
        {
            HttpStatusCode lastStatusCode;

            try
            {
                return await this.GetSongUrlInternalAsync(song, forceSwitch: false);
            }
            catch (WebRequestException e)
            {
                if (e.StatusCode != HttpStatusCode.Forbidden
                    && e.StatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }

                lastStatusCode = e.StatusCode;
            }

            if (lastStatusCode == HttpStatusCode.Forbidden)
            {
                try
                {
                    await this.UpdateGoogleKeyAsync();
                }
                catch (Exception e)
                {
                    this.logger.Warning("Could not update google key from dropbox.");
                }

                return await this.GetSongUrlInternalAsync(song, forceSwitch: false);
            }
            else
            {
                return await this.GetSongUrlInternalAsync(song, forceSwitch: true);
            }
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

        private async Task<GoogleMusicSongUrl> GetSongUrlInternalAsync(Song song, bool forceSwitch)
        {
            string url = null;

            bool useSignature = !string.IsNullOrEmpty(song.StoreId) 
                                && song.StreamType != StreamType.Free
                                && song.StreamType != StreamType.OwnLibrary;

            if (forceSwitch)
            {
                useSignature = !useSignature;
            }

            if (useSignature && !string.IsNullOrEmpty(song.StoreId))
            {
                StringBuilder salt = new StringBuilder(12);
                Random r = new Random((int)DateTime.Now.Ticks);

                for (int i = 0; i < 12; i++)
                {
                    salt.Append(AlpanumLowercase[r.Next(AlpanumLowercase.Length)]);
                }

                var hash = this.Hash(song.StoreId + salt, this.GetKey()).ToCharArray();

                for (int i = 0; i < hash.Length; i++)
                {
                    if (hash[i] == '+')
                    {
                        hash[i] = '-';
                    }
                    else if (hash[i] == '/')
                    {
                        hash[i] = '_';
                    }
                    else if (hash[i] == '=')
                    {
                        hash[i] = '.';
                    }
                }

                string hashString = new string(hash);

                url = string.Format(SongUrlFromStoreFormat, song.StoreId, salt, hashString);
            }
            else
            {
                url = string.Format(SongUrlFormat, song.ProviderSongId);
            }

            return await this.googleMusicWebService.GetAsync<GoogleMusicSongUrl>(url, signUrl: false);
        }

        private string Hash(string value, string key)
        {
            var provider = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);
            var hmacKey = provider.CreateKey(CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8));
            var signedData = CryptographicEngine.Sign(hmacKey, CryptographicBuffer.ConvertStringToBinary(value, BinaryStringEncoding.Utf8));

            return Convert.ToBase64String(signedData.ToArray());
        }

        private async Task UpdateGoogleKeyAsync()
        {
            HttpClient client = new HttpClient();
            var result = await client.GetStringAsync("https://dl.dropboxusercontent.com/u/114202641/gMusicW/aakey.txt");

            this.settingsService.SetValue(SettingsGoogleKeyName, result);
        }

        private string GetKey()
        {
            return this.settingsService.GetValue(SettingsGoogleKeyName, DefaultGoogleKey);
        }
    }
}