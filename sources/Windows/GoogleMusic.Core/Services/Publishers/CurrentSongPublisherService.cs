﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services.Publishers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.Storage;

    public class CurrentSongPublisherService : ICurrentSongPublisherService
    {
        private const string DelayPublishersSettingsKey = "DelayPublishersHoldUp";

        private const string AlbumArtCacheFolder = "AlbumArtCache";
        private const string CurrentAlbumArtFile = "current.jpg";

        private readonly object locker = new object();

        private readonly ILogger logger;
        private readonly ISettingsService settingsService;
        private readonly List<Lazy<ICurrentSongPublisher>> songPublishers = new List<Lazy<ICurrentSongPublisher>>();

        private readonly HttpClient httpImageDownloadClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };

        private readonly int delayPublishersHoldUp;

        private CancellationTokenSource cancellationTokenSource;

        public CurrentSongPublisherService(ILogManager logManager, ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            this.logger = logManager.CreateLogger("CurrentSongPublisherService");

            this.delayPublishersHoldUp = this.settingsService.GetValue(DelayPublishersSettingsKey, defaultValue: 15000);
        }

        public void AddPublisher(Lazy<ICurrentSongPublisher> publisher)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Adding new pubslisher: {0}.", publisher.GetType());
            }

            this.songPublishers.Add(publisher);
        }

        public void AddPublisher(ICurrentSongPublisher publisher)
        {
            this.AddPublisher(new Lazy<ICurrentSongPublisher>(() => publisher));
        }

        public async Task PublishAsync(Song song, Playlist currentPlaylist)
        {
            CancellationTokenSource source;

            lock (this.locker)
            {
                this.CancelActiveTasks();

                this.cancellationTokenSource = source = new CancellationTokenSource();
            }

            await this.PublishAsync(song, currentPlaylist, source.Token);
        }

        public void CancelActiveTasks()
        {
            lock (this.locker)
            {
                if (this.cancellationTokenSource != null)
                {
                    this.cancellationTokenSource.Cancel();
                    this.cancellationTokenSource = null;
                }
            }
        }

        private async Task PublishAsync(Song song, Playlist currentPlaylist, CancellationToken cancellationToken)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("PublishAsync: Add new task for publishing: SongId: {0}, PlaylistType: {1}.", song.GoogleMusicMetadata.Id, currentPlaylist.GetType());
            }

            Task<Uri> getAlbumArtTak = this.GetAlbumArtUri(song);

            cancellationToken.ThrowIfCancellationRequested();

            Task immediately = Task.Factory.StartNew(
                async () =>
                    {
                        Task withoutAlbumArt = this.PublishAsync(
                            this.songPublishers.Where(x => x.Value.PublisherType == PublisherType.Immediately),
                            song,
                            currentPlaylist,
                            null,
                            cancellationToken);

                        Uri albumArt = await getAlbumArtTak;

                        Task withAlbumArt = this.PublishAsync(
                            this.songPublishers.Where(x => x.Value.PublisherType == PublisherType.ImmediatelyWithAlbumArt),
                            song,
                            currentPlaylist,
                            albumArt,
                            cancellationToken);

                        await Task.WhenAll(withAlbumArt, withoutAlbumArt);
                    });

            cancellationToken.ThrowIfCancellationRequested();

            var delayPublishers =
                this.songPublishers.Where(
                    x => x.Value.PublisherType == PublisherType.Delay || x.Value.PublisherType == PublisherType.DelayWithAlbumArt).ToList();

            if (!delayPublishers.Any())
            {
                this.logger.Debug("PublishAsync: no delay publishers, return.", song.GoogleMusicMetadata.Id, currentPlaylist.GetType());
                return;
            }

            await Task.Delay(Math.Min(this.delayPublishersHoldUp, (int)(0.3 * (double)song.GoogleMusicMetadata.DurationMillis)), cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            Task delay = Task.Factory.StartNew(
                async () =>
                    {
                        Task withoutAlbumArt = this.PublishAsync(
                            this.songPublishers.Where(x => x.Value.PublisherType == PublisherType.Delay),
                            song,
                            currentPlaylist,
                            null,
                            cancellationToken);

                        Uri albumArt = await getAlbumArtTak;

                        Task withAlbumArt = this.PublishAsync(
                            this.songPublishers.Where(x => x.Value.PublisherType == PublisherType.DelayWithAlbumArt),
                            song,
                            currentPlaylist,
                            albumArt,
                            cancellationToken);

                        await Task.WhenAll(withAlbumArt, withoutAlbumArt);
                    });

            cancellationToken.ThrowIfCancellationRequested();

            await Task.WhenAll(immediately, delay);

            this.logger.Debug("PublishAsync completed for SongId: {0}, PlaylistType: {1}.", song.GoogleMusicMetadata.Id, currentPlaylist.GetType());
        }

        private async Task PublishAsync(IEnumerable<Lazy<ICurrentSongPublisher>> publishers, Song song, Playlist currentPlaylist, Uri albumArtUri, CancellationToken cancellationToken)
        {
            await Task.WhenAll(publishers.Select(x => x.Value.PublishAsync(song, currentPlaylist, albumArtUri, cancellationToken)).Where(task => task != null));
        }

        private async Task<Uri> GetAlbumArtUri(Song song)
        {
            Uri albumArtUri = new Uri("ms-appx:///Assets/Logo.png");

            try
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("Song album art: {0}.", song.GoogleMusicMetadata.AlbumArtUrl);
                }

                if (!string.IsNullOrEmpty(song.GoogleMusicMetadata.AlbumArtUrl))
                {
                    byte[] bytes = await this.httpImageDownloadClient.GetByteArrayAsync("http:" + song.GoogleMusicMetadata.AlbumArtUrl);
                    var localFolder = ApplicationData.Current.LocalFolder;

                    var folder = await localFolder.CreateFolderAsync(AlbumArtCacheFolder, CreationCollisionOption.OpenIfExists);

                    var file = await folder.CreateFileAsync(CurrentAlbumArtFile, CreationCollisionOption.ReplaceExisting);

                    await FileIO.WriteBytesAsync(file, bytes);

                    albumArtUri = new Uri(string.Format(CultureInfo.InvariantCulture, "ms-appdata:///local/{0}/{1}", AlbumArtCacheFolder, CurrentAlbumArtFile));
                }
            }
            catch (Exception exception)
            {
                this.logger.Error("Cannot download album art.");
                this.logger.LogErrorException(exception);
            }

            return albumArtUri;
        }
    }
}