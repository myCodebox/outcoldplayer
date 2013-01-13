// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services.Publishers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;

    public class CurrentSongPublisherService : ICurrentSongPublisherService
    {
        private const string DelayPublishersSettingsKey = "DelayPublishersHoldUp";

        private readonly object locker = new object();

        private readonly ILogger logger;
        private readonly ISettingsService settingsService;
        private readonly List<Lazy<ICurrentSongPublisher>> songPublishers = new List<Lazy<ICurrentSongPublisher>>();

        private readonly int delayPublishersHoldUp;

        private CancellationTokenSource cancellationTokenSource;

        public CurrentSongPublisherService(ILogManager logManager, ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            this.logger = logManager.CreateLogger("CurrentSongPublisherService");

            this.delayPublishersHoldUp = this.settingsService.GetValue(DelayPublishersSettingsKey, defaultValue: 10000);
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

            cancellationToken.ThrowIfCancellationRequested();

            Task immediately = this.PublishAsync(this.songPublishers.Where(x => x.Value.PublisherType == PublisherType.Immediately), song, currentPlaylist, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var delayPublishers = this.songPublishers.Where(x => x.Value.PublisherType == PublisherType.Delay).ToList();

            if (!delayPublishers.Any())
            {
                this.logger.Debug("PublishAsync: no delay publishers, return.", song.GoogleMusicMetadata.Id, currentPlaylist.GetType());
                return;
            }

            await Task.Delay(this.delayPublishersHoldUp, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            Task delay = this.PublishAsync(delayPublishers, song, currentPlaylist, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await Task.WhenAll(immediately, delay);

            this.logger.Debug("PublishAsync completed for SongId: {0}, PlaylistType: {1}.", song.GoogleMusicMetadata.Id, currentPlaylist.GetType());
        }

        private async Task PublishAsync(IEnumerable<Lazy<ICurrentSongPublisher>> publishers, Song song, Playlist currentPlaylist, CancellationToken cancellationToken)
        {
            await Task.WhenAll(publishers.Select(x => x.Value.PublishAsync(song, currentPlaylist, cancellationToken)).Where(task => task != null));
        }
    }
}