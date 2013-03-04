// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services.Publishers;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class PlayQueueService : IPlayQueueService
    {
        private readonly ILogger logger;
        private readonly IMediaElementContainer mediaElement;
        private readonly ISettingsService settingsService;
        private readonly ICurrentSongPublisherService publisherService;
        private readonly ISongWebService songWebService;
        private readonly INotificationService notificationService;

        private readonly List<Song> songsQueue = new List<Song>();
        private readonly List<int> queueOrder = new List<int>();

        private readonly Random random = new Random((int)DateTime.Now.Ticks);

        private readonly IMediaStreamDownloadService downloadService;

        private INetworkRandomAccessStream currentSongStream;
        private int currentQueueIndex; // From queueOrder

        private Playlist currentPlaylist;

        private QueueState state;

        public PlayQueueService(
            ILogManager logManager,
            IMediaElementContainer mediaElement,
            ISettingsService settingsService,
            IMediaStreamDownloadService downloadService,
            ICurrentSongPublisherService publisherService,
            ISongWebService songWebService,
            INotificationService notificationService,
            IGoogleMusicSessionService sessionService)
        {
            this.logger = logManager.CreateLogger("PlayQueueService");
            this.mediaElement = mediaElement;
            this.settingsService = settingsService;
            this.downloadService = downloadService;
            this.publisherService = publisherService;
            this.songWebService = songWebService;
            this.notificationService = notificationService;
            this.currentQueueIndex = -1;

            this.IsRepeatAll = this.settingsService.GetValue("IsRepeatAllEnabled", defaultValue: false);
            this.IsShuffled = this.settingsService.GetValue("IsShuffleEnabled", defaultValue: false);

            this.State = QueueState.Unknown;

            this.mediaElement.MediaEnded += async (sender, args) =>
                {
                    if (this.CanSwitchToNext())
                    {
                        await this.NextSongAsync();
                    }
                };

            sessionService.SessionCleared += async (sender, args) =>
                {
                    await this.StopAsync();
                    this.queueOrder.Clear();
                    this.songsQueue.Clear();
                    this.currentQueueIndex = -1;
                    this.currentSongStream = null;
                    this.currentPlaylist = null;
                };
        }

        public event EventHandler QueueChanged;

        public event EventHandler<StateChangedEventArgs> StateChanged;

        public event EventHandler<double> DownloadProgress;

        public bool IsShuffled { get; private set; }

        public bool IsRepeatAll { get; private set; }

        public QueueState State
        {
            get
            {
                return this.state;
            }

            private set
            {
                this.state = value;

                if (value == QueueState.Play || value == QueueState.Paused)
                {
                    this.RaiseStateChanged(new StateChangedEventArgs(value, this.songsQueue[this.CurrentSongIndex]));
                }
                else
                {
                    this.RaiseStateChanged(new StateChangedEventArgs(value));
                }
            }
        }

        private int CurrentSongIndex
        {
            get
            {
                if (this.currentQueueIndex >= this.queueOrder.Count)
                {
                    return -1;
                }

                return this.queueOrder[this.currentQueueIndex];
            }
        }

        public async Task PlayAsync(Playlist playlist)
        {
            await this.PlayAsync(playlist, songIndex: -1);
        }

        public async Task PlayAsync(Playlist playlist, int songIndex)
        {
            if (playlist == null)
            {
                throw new ArgumentNullException("playlist");
            }

            await Task.Run(async () =>
            {
                if (this.State == QueueState.Busy)
                {
                    throw new InvalidOperationException("Queue is busy");
                }

                this.currentPlaylist = playlist;
                this.songsQueue.Clear();
                this.songsQueue.AddRange(playlist.Songs);

                this.UpdateOrder(songIndex);

                if (this.currentQueueIndex >= 0)
                {
                    await this.PlaySongAsyncInternal(this.CurrentSongIndex);
                }
                else
                {
                    await this.StopAsync();
                }
            });
        }

        public async Task PlayAsync(int songIndex)
        {
            await Task.Run(async () =>
            {
                if (this.State == QueueState.Busy)
                {
                    throw new InvalidOperationException("Queue is busy");
                }

                this.currentQueueIndex = this.queueOrder.IndexOf(songIndex);

                await this.PlaySongAsyncInternal(this.CurrentSongIndex);
            });
        }

        public async Task PlayAsync()
        {
            if (this.State == QueueState.Busy)
            {
                throw new InvalidOperationException("Queue is busy");
            }

            if (this.State == QueueState.Paused)
            {
                await this.mediaElement.PlayAsync();
                this.State = QueueState.Play;
                this.logger.LogTask(this.publisherService.PublishAsync(this.songsQueue[this.CurrentSongIndex], this.currentPlaylist));
            }
            else
            {
                await this.PlaySongAsyncInternal(this.CurrentSongIndex);
            }
        }

        public async Task StopAsync()
        {
            if (this.State == QueueState.Play || this.State == QueueState.Paused)
            {
                this.publisherService.CancelActiveTasks();
                await this.mediaElement.StopAsync();
                this.State = QueueState.Stopped;
            }
        }

        public async Task NextSongAsync()
        {
            if (this.State == QueueState.Busy)
            {
                throw new InvalidOperationException("Queue is busy");
            }

            if (this.currentQueueIndex == (this.queueOrder.Count - 1) && this.IsRepeatAll)
            {
                this.currentQueueIndex = 0;
            }
            else
            {
                this.currentQueueIndex++;
            }

            await this.PlaySongAsyncInternal(this.currentQueueIndex);
        }

        public bool CanSwitchToNext()
        {
            return this.currentQueueIndex < (this.queueOrder.Count - 1) || this.IsRepeatAll;
        }

        public async Task PreviousSongAsync()
        {
            if (this.State == QueueState.Busy)
            {
                throw new InvalidOperationException("Queue is busy");
            }

            if (this.currentQueueIndex != 0)
            {
                this.currentQueueIndex--;
            }
            else if (this.IsRepeatAll)
            {
                this.currentQueueIndex = this.queueOrder.Count - 1;
            }

            await this.PlaySongAsyncInternal(this.currentQueueIndex);
        }

        public bool CanSwitchToPrevious()
        {
            return this.currentQueueIndex > 1 || this.IsRepeatAll;
        }

        public async Task PauseAsync()
        {
            if (this.State == QueueState.Play)
            {
                this.publisherService.CancelActiveTasks();
                await this.mediaElement.PauseAsync();
                this.State = QueueState.Paused;
            }
        }

        public async Task AddRangeAsync(IEnumerable<Song> songs)
        {
            if (songs == null)
            {
                throw new ArgumentNullException("songs");
            }

            await Task.Run(() =>
            {
                this.currentPlaylist = null;

                var addedSongs = songs.ToList();
                
                var range = Enumerable.Range(this.songsQueue.Count, this.songsQueue.Count + addedSongs.Count);
                this.songsQueue.AddRange(addedSongs);

                if (this.IsShuffled)
                {
                    var shuffledQueue =
                        range.Select(
                            x => new
                            {
                                OrderIndex = this.random.Next(),
                                SongIndex = x
                            })
                            .OrderBy(x => x.OrderIndex)
                            .Select(x => x.SongIndex);

                    this.queueOrder.AddRange(shuffledQueue);
                }
                else
                {
                    this.queueOrder.AddRange(range);
                }
            });
        }

        public async Task RemoveAsync(int songIndex)
        {
            await Task.Run(async () =>
            {
                if (this.State == QueueState.Busy)
                {
                    throw new InvalidOperationException("Queue is busy");
                }

                if (songIndex >= this.songsQueue.Count)
                {
                    throw new ArgumentOutOfRangeException("songIndex");
                }

                this.currentPlaylist = null;

                if (this.songsQueue.Count > songIndex)
                {
                    var queueIndex = this.queueOrder.IndexOf(songIndex);

                    this.queueOrder.RemoveAt(queueIndex);

                    for (int i = 0; i < this.queueOrder.Count; i++)
                    {
                        if (this.queueOrder[i] > songIndex)
                        {
                            this.queueOrder[i]--;
                        }
                    }

                    this.songsQueue.RemoveAt(songIndex);

                    if (queueIndex < this.currentQueueIndex)
                    {
                        this.currentQueueIndex--;
                    }

                    if (this.songsQueue.Count == 0)
                    {
                        await this.mediaElement.StopAsync();
                        this.currentQueueIndex = -1;
                    }
                    else
                    {
                        await this.PlaySongAsyncInternal(this.CurrentSongIndex);
                    }

                    this.RaiseQueueChanged();
                }
            });
        }

        public async Task SetRepeatAllAsync(bool repeatAll)
        {
            await Task.Run(() =>
                {
                    if (this.IsRepeatAll != repeatAll)
                    {
                        this.IsRepeatAll = repeatAll;
                        this.settingsService.SetValue("IsRepeatAllEnabled", this.IsRepeatAll);
                        this.RaiseQueueChanged();
                    }
                });
        }

        public async Task SetShuffledAsync(bool isShuffled)
        {
            await Task.Run(() =>
                {
                    if (isShuffled != this.IsShuffled)
                    {
                        this.IsShuffled = isShuffled;
                        this.settingsService.SetValue("IsShuffleEnabled", this.IsShuffled);

                        lock (this.queueOrder)
                        {
                            this.UpdateOrder();
                        }

                        this.RaiseQueueChanged();
                    }
                });
        }

        public IEnumerable<Song> GetQueue()
        {
            lock (this.queueOrder)
            {
                return this.songsQueue.ToList();
            }
        }

        public int GetCurrentSongIndex()
        {
            return this.CurrentSongIndex;
        }

        private async Task PlaySongAsyncInternal(int songIndex)
        {
            var queueIndex = this.queueOrder.IndexOf(songIndex);

            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Play current song index: {0}.", queueIndex);
            }

            this.State = QueueState.Busy;

            this.publisherService.CancelActiveTasks();

            if (this.queueOrder.Count > queueIndex)
            {
                var song = this.songsQueue[songIndex];
                if (song != null)
                {
                    if (this.logger.IsDebugEnabled)
                    {
                        this.logger.Debug("Getting url for song '{0}'.", song.Metadata.Id);
                    }

                    GoogleMusicSongUrl songUrl = null;

                    try
                    {
                        songUrl = await this.songWebService.GetSongUrlAsync(song.Metadata.Id);
                    }
                    catch (Exception e)
                    {
                        this.logger.LogErrorException(e);
                    }

                    if (songUrl != null)
                    {
                        if (this.logger.IsDebugEnabled)
                        {
                            this.logger.Debug("Getting stream by url '{0}'.", songUrl.Url);
                        }

                        if (this.currentSongStream != null)
                        {
                            this.logger.Debug("Current song is not null. Disposing current stream.");

                            await this.mediaElement.StopAsync();

                            this.currentSongStream.DownloadProgressChanged -= this.CurrentSongStreamOnDownloadProgressChanged;
                            this.currentSongStream.Dispose();
                            this.currentSongStream = null;
                        }

                        var stream = await this.downloadService.GetStreamAsync(songUrl.Url);

                        this.currentSongStream = stream;
                        this.currentSongStream.DownloadProgressChanged += this.CurrentSongStreamOnDownloadProgressChanged;

                        await this.mediaElement.PlayAsync(this.currentSongStream, this.currentSongStream.ContentType);

                        this.State = QueueState.Play;

                        this.logger.LogTask(this.publisherService.PublishAsync(song, this.currentPlaylist));
                    }
                    else
                    {
                        this.State = QueueState.Stopped;

                        if (this.logger.IsDebugEnabled)
                        {
                            this.logger.Debug("Could not get url for song {0}.", song.Metadata.Id);
                        }

                        this.logger.LogTask(this.notificationService.ShowMessageAsync("Cannot play right now. Make sure that you don't use current account on different device at the same time. Try after couple minutes."));
                    }
                }
            }
            else
            {
                this.State = QueueState.Stopped;
            }
        }

        private void UpdateOrder()
        {
            this.UpdateOrder(this.CurrentSongIndex);
        }

        private void UpdateOrder(int firstSongIndex)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Update order.");
            }

            this.queueOrder.Clear();

            if (this.songsQueue.Count > 0)
            {
                var range = Enumerable.Range(0, this.songsQueue.Count);

                if (this.IsShuffled)
                {
                    var shuffledQueue =
                        range.Select(
                            x => new
                                        {
                                            OrderIndex = (x == firstSongIndex) ? -1 : this.random.Next(),
                                            SongIndex = x
                                        })
                                .OrderBy(x => x.OrderIndex)
                                .Select(x => x.SongIndex);

                    this.queueOrder.AddRange(shuffledQueue);
                }
                else
                {
                    this.queueOrder.AddRange(range);
                }

                if (this.logger.IsInfoEnabled)
                {
                    this.logger.Info("Shuffle enabled: {0}", this.IsShuffled);
                    this.logger.Info("Playing order: {0}", string.Join(",", this.queueOrder));
                }

                this.currentQueueIndex = firstSongIndex >= 0 ? this.queueOrder.IndexOf(firstSongIndex) : 0;
            }
            else
            {
                this.currentQueueIndex = -1;
            }
        }

        private void CurrentSongStreamOnDownloadProgressChanged(object sender, double e)
        {
            this.RaiseDownloadProgress(e);
        }

        private void RaiseStateChanged(StateChangedEventArgs e)
        {
            var handler = this.StateChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void RaiseDownloadProgress(double e)
        {
            var handler = this.DownloadProgress;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void RaiseQueueChanged()
        {
            var handler = this.QueueChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}