// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Text;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    using Windows.Security.Cryptography;
    using Windows.Security.Cryptography.Core;

    public interface ISongsWebService
    {
        Task<StatusResp> GetStatusAsync();

        Task<IList<GoogleMusicSong>> GetAllAsync(
            DateTime? lastUpdate,
            IProgress<int> progress,
            Func<IList<GoogleMusicSong>, Task> chunkHandler = null);

        Task<GoogleMusicSongUrl> GetSongUrlAsync(Song song);

        Task<GoogleMusicTrackStatResponse> SendStatsAsync(IList<Song> songs);

        Task<GoogleMusicSongMutateResponse> UpdateRatingsAsync(IDictionary<Song, int> ratings);
    }

    public class SongsWebService : ISongsWebService
    {
        private const string AlpanumLowercase = "abcdefghijklmnopqrstuvwxyz" + "0123456789";
        private const string DefaultGoogleKey = "27f7313e-f75d-445a-ac99-56386a5fe879";

        private const string SettingsGoogleKeyName = "GoogleAAKey";

        private const string SongUrlFormat = "play?songid={0}&pt=e&dt=pe&targetkbps=320&start=0";
        private const string SongUrlFromStoreFormat = "play?mjck={0}&slt={1}&sig={2}&pt=e&dt=pe&targetkbps=320&start=0";

        private const string TrackStats = "trackstats";
        private const string TrackBatch = "trackbatch";
        
        private const string GetStatusUrl = "services/getstatus";

        private const string TrackFeed = "trackfeed?art-dimension=512";

        private readonly IGoogleMusicWebService googleMusicWebService;

        private readonly IGoogleMusicApisService googleMusicApisService;

        private readonly ISettingsService settingsService;
        private readonly ILogger logger;

        public SongsWebService(
            IGoogleMusicWebService googleMusicWebService,
            IGoogleMusicApisService googleMusicApisService,
            ISettingsService settingsService,
            ILogManager logManager)
        {
            this.googleMusicWebService = googleMusicWebService;
            this.googleMusicApisService = googleMusicApisService;
            this.settingsService = settingsService;
            this.logger = logManager.CreateLogger("SongsWebService");
        }

        public async Task<StatusResp> GetStatusAsync()
        {
            return await this.googleMusicWebService.PostAsync<StatusResp>(GetStatusUrl, forceJsonBody: false);
        }

        public Task<IList<GoogleMusicSong>> GetAllAsync(DateTime? lastUpdate, IProgress<int> progress, Func<IList<GoogleMusicSong>, Task> chunkHandler = null)
        {
            return this.googleMusicApisService.DownloadList(TrackFeed, lastUpdate, progress, chunkHandler);
        }

        public async Task<GoogleMusicSongUrl> GetSongUrlAsync(Song song)
        {
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

                this.logger.Debug(e, "Tried to get song url by first attempt");
            }

            try
            {
                return await this.GetSongUrlInternalAsync(song, forceSwitch: true);
            }
            catch (WebRequestException e)
            {
                if (e.StatusCode != HttpStatusCode.Forbidden
                    && e.StatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }

                this.logger.Debug(e, "Tried to get song url by second attempt (with force switch)");
            }

            try
            {
                await this.UpdateGoogleKeyAsync();
            }
            catch (Exception e)
            {
                this.logger.Warning(e, "Could not update google key from dropbox.");
            }

            return await this.GetSongUrlInternalAsync(song, forceSwitch: false);
        }

        public async Task<GoogleMusicTrackStatResponse> SendStatsAsync(IList<Song> songs)
        {
            var json = new
                       {
                           track_stats = songs.Select(
                               x =>
                               new 
                               {
                                   id = x.SongId,
                                   incremental_plays = x.StatsPlayCount,
                                   last_play_time_millis = ((long)x.StatsRecent.ToUnixFileTime() * 1000L).ToString("G"),
                                   type = x.TrackType == StreamType.EphemeralSubscription ? 2 : 1
                               })
                       };

            return await this.googleMusicApisService.PostAsync<GoogleMusicTrackStatResponse>(TrackStats, json);
        }

        public async Task<GoogleMusicSongMutateResponse> UpdateRatingsAsync(IDictionary<Song, int> ratings)
        {
            var json =
                new
                {
                    mutations = ratings.Select(update =>
                                new
                                {
                                    update =
                                        new
                                        {
                                            id = update.Key.SongId,
                                            nid = update.Key.Nid,
                                            rating = update.Value.ToString(),
                                            trackType = (int)update.Key.TrackType,
                                            deleted = false,
                                            lastModifiedTimestamp = ((long)DateTime.UtcNow.ToUnixFileTime() * 1000L).ToString("G")
                                        }
                                })
                };

            return await this.googleMusicApisService.PostAsync<GoogleMusicSongMutateResponse>(TrackBatch, json);
        }

        private async Task<GoogleMusicSongUrl> GetSongUrlInternalAsync(Song song, bool forceSwitch)
        {
            string url = null;

            bool useSignature = !string.IsNullOrEmpty(song.StoreId) 
                                && song.TrackType != StreamType.Free
                                && song.TrackType != StreamType.OwnLibrary;

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
                url = string.Format(SongUrlFormat, song.SongId);
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