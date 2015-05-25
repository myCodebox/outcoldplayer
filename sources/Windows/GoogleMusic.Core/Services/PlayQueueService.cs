// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.EventAggregator;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services.Publishers;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Web;

    using Windows.Storage.Streams;

    public class PlayQueueService : IPlayQueueService
    {
        private readonly ILogger logger;
        private readonly IMediaElementContainer mediaElement;
        private readonly ISettingsService settingsService;
        private readonly ISongsCachingService songsCachingService;
        private readonly ICurrentSongPublisherService publisherService;
        private readonly IPlaylistsService playlistsService;
        private readonly IRadioStationsService radioStationsService;

        private readonly IEventAggregator eventAggregator;

        private readonly IAnalyticsService analyticsService;

        private readonly List<Song> songsQueue = new List<Song>();
        private readonly List<int> queueOrder = new List<int>();

        private readonly Random random = new Random((int)DateTime.Now.Ticks);

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

        private IRandomAccessStream currentSongStream;
        private Song currentSong;

        private CancellationTokenSource currentTokenSource;
        private CancellationTokenSource predownloadTokenSource;

        private int currentQueueIndex; // From queueOrder

        private QueueState state;

        private bool isShuffled;

        private RepeatType repeat;

        public PlayQueueService(
            ILogManager logManager,
            IMediaElementContainer mediaElement,
            ISettingsService settingsService,
            ISongsCachingService songsCachingService,
            ICurrentSongPublisherService publisherService,
            IGoogleMusicSessionService sessionService,
            IPlaylistsService playlistsService,
            IRadioStationsService radioStationsService,
            IEventAggregator eventAggregator,
            IAnalyticsService analyticsService)
        {
            this.logger = logManager.CreateLogger("PlayQueueService");
            this.mediaElement = mediaElement;
            this.settingsService = settingsService;
            this.songsCachingService = songsCachingService;
            this.publisherService = publisherService;
            this.playlistsService = playlistsService;
            this.radioStationsService = radioStationsService;
            this.eventAggregator = eventAggregator;
            this.analyticsService = analyticsService;
            this.currentQueueIndex = -1;

            this.Repeat = this.settingsService.GetValue("RepeatValue", defaultValue: RepeatType.None);
            this.IsShuffled = this.settingsService.GetValue("IsShuffleEnabled", defaultValue: false);

            this.State = QueueState.Unknown;

            this.mediaElement.MediaEnded += async (sender, args) =>
                {
                    if (this.CanSwitchToNext())
                    {
                        await this.NextSongAsync();
                    }
                    else
                    {
                        this.State = QueueState.Stopped;
                    }
                };

            sessionService.SessionCleared += async (sender, args) => { await ClearQueueAsync(); };
            eventAggregator.GetEvent<ReloadSongsEvent>().Subscribe(async (e) => { await ClearQueueAsync(); });
        }

        public event EventHandler QueueChanged;

        public event EventHandler<StateChangedEventArgs> StateChanged;

        public event EventHandler<double> DownloadProgress;

        public bool IsShuffled
        {
            get
            {
                return this.isShuffled && !this.IsRadio;
            }
            
            set
            {
                if (this.isShuffled != value)
                {
                    this.analyticsService.SendEvent("Media", "IsShuffled", value.ToString());

                    this.isShuffled = value;
                    this.settingsService.SetValue("IsShuffleEnabled", this.isShuffled);

                    lock (this.queueOrder)
                    {
                        this.UpdateOrder();
                    }

                    this.RaiseQueueChanged();
                }
            }
        }

        public RepeatType Repeat
        {
            get
            {
                return this.IsRadio ? RepeatType.None : this.repeat;
            }
            
            set
            {
                if (this.repeat != value)
                {
                    this.analyticsService.SendEvent("Media", "Repeat", value.ToString());

                    this.repeat = value;
                    this.settingsService.SetValue("RepeatValue", this.repeat);
                    this.RaiseQueueChanged();
                }
            }
        }

        public bool IsRadio
        {
            get
            {
                return this.CurrentPlaylist != null && this.CurrentPlaylist.PlaylistType == PlaylistType.Radio;
            }
        }

        public IPlaylist CurrentPlaylist { get; private set; }

        public QueueState State
        {
            get
            {
                return this.state;
            }

            private set
            {
                this.state = value;

                this.RaiseStateChanged(new StateChangedEventArgs(value, (value == QueueState.Stopped || value == QueueState.Unknown) ? null : this.GetCurrentSong()));
            }
        }

        private int CurrentSongIndex
        {
            get
            {
                if (this.currentQueueIndex >= this.queueOrder.Count || this.currentQueueIndex < 0)
                {
                    return -1;
                }

                return this.queueOrder[this.currentQueueIndex];
            }
        }

        public async Task<bool> PlayAsync(IPlaylist playlist)
        {
            return await this.PlayAsync(playlist, songIndex: -1);
        }

        public async Task<bool> PlayAsync(IPlaylist playlist, int songIndex)
        {
            if (playlist == null)
            {
                throw new ArgumentNullException("playlist");
            }

            this.analyticsService.SendEvent("Media", "Play", playlist.PlaylistType.ToString("G"));

            var songs = await this.playlistsService.GetSongsAsync(playlist);
            if (songs != null)
            {
                return await this.PlayAsync(playlist, songs, songIndex);
            }

            return false;
        }

        public async Task<bool> PlayAsync(IPlaylist playlist, IEnumerable<Song> songs, int songIndex)
        {
            return await Task.Run(async () =>
            {
                this.CurrentPlaylist = playlist;

                this.analyticsService.SendEvent("Media", "Play", playlist == null ? "Songs" : playlist.PlaylistType.ToString("G"));

                this.songsQueue.Clear();
                this.songsQueue.AddRange(songs);

                this.RaiseQueueChanged();

                this.UpdateOrder(songIndex);

                if (this.currentQueueIndex >= 0)
                {
                    return await this.PlaySongAsyncInternal(this.CurrentSongIndex);
                }
                else
                {
                    await this.StopAsync();
                    return true;
                }
            });
        }

        public async Task<bool> PlayAsync(int songIndex)
        {
            return await Task.Run(async () =>
            {
                this.currentQueueIndex = this.queueOrder.IndexOf(songIndex);

                return await this.PlaySongAsyncInternal(this.CurrentSongIndex);
            });
        }

        public Task<bool> PlayAsync(IEnumerable<Song> songs)
        {
            return this.PlayAsync(null, songs, songIndex: -1);
        }

        public async Task<bool> PlayAsync()
        {
            if (this.State == QueueState.Paused)
            {
                this.analyticsService.SendEvent("Media", "Queue", "Unpause");

                await this.mediaElement.PlayAsync();
                this.State = QueueState.Play;
                this.logger.LogTask(this.publisherService.PublishAsync(this.songsQueue[this.CurrentSongIndex], this.CurrentPlaylist));
                return true;
            }
            else
            {
                this.analyticsService.SendEvent("Media", "Queue", "Play current");

                return await this.PlaySongAsyncInternal(this.CurrentSongIndex);
            }
        }

        public async Task StopAsync()
        {
            if (this.State == QueueState.Play || this.State == QueueState.Paused)
            {
                this.analyticsService.SendEvent("Media", "Queue", "Stop");

                this.publisherService.CancelActiveTasks();
                await this.mediaElement.StopAsync();
                this.State = this.queueOrder.Count > 0 ? QueueState.Stopped : QueueState.Unknown;
            }
        }

        public async Task<bool> NextSongAsync()
        {
            this.analyticsService.SendEvent("Media", "Queue", "Next");

            if (this.CanSwitchToNext())
            {
                if (this.Repeat == RepeatType.One)
                {
                    // Just keep the same song
                }
                else if (this.currentQueueIndex == (this.queueOrder.Count - 1) && this.Repeat == RepeatType.All)
                {
                    this.currentQueueIndex = 0;
                }
                else
                {
                    this.currentQueueIndex++;
                }

                return await this.PlaySongAsyncInternal(this.CurrentSongIndex, nextSong: true);
            }

            return false;
        }

        public bool CanSwitchToNext()
        {
            return this.currentQueueIndex < (this.queueOrder.Count - 1) || (this.Repeat != RepeatType.None && this.queueOrder.Count > 0);
        }

        public async Task<bool> PreviousSongAsync()
        {
            this.analyticsService.SendEvent("Media", "Queue", "Previous");

            if (this.CanSwitchToPrevious())
            {
                if (this.Repeat == RepeatType.One || !(await this.mediaElement.IsBeginning()))
                {
                    // Just keep the same song
                }
                else if (this.currentQueueIndex != 0)
                {
                    this.currentQueueIndex--;
                }
                else if (this.Repeat == RepeatType.All)
                {
                    this.currentQueueIndex = this.queueOrder.Count - 1;
                }

                return await this.PlaySongAsyncInternal(this.CurrentSongIndex);
            }

            return false;
        }

        public bool CanSwitchToPrevious()
        {
            return this.currentQueueIndex >= 0 || (this.Repeat != RepeatType.None && this.queueOrder.Count > 0);
        }

        public async Task PauseAsync()
        {
            this.analyticsService.SendEvent("Media", "Queue", "Pause");

            if (this.State == QueueState.Play)
            {
                this.publisherService.CancelActiveTasks();
                await this.mediaElement.PauseAsync();
                this.State = QueueState.Paused;
            }
        }

        public Task AddRangeAsync(IEnumerable<Song> songs, bool playNext)
        {
            return this.AddRangeAsync(null, songs, playNext);
        }

        public async Task AddRangeAsync(IPlaylist playlist, IEnumerable<Song> songs, bool playNext)
        {
            if (songs == null)
            {
                throw new ArgumentNullException("songs");
            }

            this.analyticsService.SendEvent("Media", "Queue", "AddRange");

            await Task.Run(() =>
            {
                this.CurrentPlaylist = playlist;

                var addedSongs = songs.ToList();

                var range = Enumerable.Range(this.songsQueue.Count, addedSongs.Count);

                if (playNext && this.queueOrder.Count > 0 && this.queueOrder.Count > (this.currentQueueIndex + 1))
                {
                    this.songsQueue.InsertRange(this.queueOrder[this.currentQueueIndex + 1], addedSongs);
                }
                else
                {
                    this.songsQueue.AddRange(addedSongs);
                }

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

                if (this.State == QueueState.Unknown && this.queueOrder.Count > 0)
                {
                    this.currentQueueIndex = 0;
                    this.State = QueueState.Stopped;
                }

                this.RaiseQueueChanged();
            });
        }

        public async Task RemoveAsync(IEnumerable<int> songIndexes)
        {
            this.analyticsService.SendEvent("Media", "Queue", "Remove songs");

            await Task.Run(async () =>
            {
                if (this.State == QueueState.Busy)
                {
                    this.logger.Debug("PlayAsync: Could not do that. Queue is busy.");
                    return;
                }

                List<int> collection = songIndexes.ToList();

                if (collection.Count > 0)
                {
                    this.CurrentPlaylist = null;

                    bool currentSongChanged = false;

                    foreach (int index in collection.OrderByDescending(x => x))
                    {
                        if (this.songsQueue.Count > index)
                        {
                            var queueIndex = this.queueOrder.IndexOf(index);

                            this.queueOrder.RemoveAt(queueIndex);

                            for (int i = 0; i < this.queueOrder.Count; i++)
                            {
                                if (this.queueOrder[i] > index)
                                {
                                    this.queueOrder[i]--;
                                }
                            }

                            this.songsQueue.RemoveAt(index);

                            if (this.songsQueue.Count == 0)
                            {
                                this.currentQueueIndex = -1;
                            }
                            else if (queueIndex == this.currentQueueIndex)
                            {
                                if (this.currentQueueIndex > 0 || this.queueOrder.Count == 0)
                                {
                                    this.currentQueueIndex--;
                                }

                                currentSongChanged = true;
                            }
                            else if (queueIndex < this.currentQueueIndex)
                            {
                                this.currentQueueIndex--;
                            }
                        }
                    }

                    if (this.currentQueueIndex == -1)
                    {
                        await this.StopAsync();
                    }
                    else if (currentSongChanged)
                    {
                        await this.PlaySongAsyncInternal(this.CurrentSongIndex);
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

        public Song GetCurrentSong()
        {
            var songIndex = this.CurrentSongIndex;
            if (songIndex < 0 || songIndex >= this.songsQueue.Count)
            {
                return null;
            }

            try
            {
                return this.songsQueue[this.CurrentSongIndex];
            }
            catch (IndexOutOfRangeException exception)
            {
                this.logger.Debug(exception, "GetCurrentSong");
                return null;
            }
        }

        private async Task<bool> PlaySongAsyncInternal(int songIndex, bool nextSong = false)
        {
            try
            {
                await this.mediaElement.StopAsync();

                var queueIndex = this.queueOrder.IndexOf(songIndex);

                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("Play current song index: {0}.", queueIndex);
                }

                this.State = QueueState.Busy;

                this.publisherService.CancelActiveTasks();

                if (this.queueOrder.Count > queueIndex && queueIndex >= 0)
                {
                    Song song = null;
                    if (this.songsQueue.Count > songIndex)
                    {
                        song = this.songsQueue[songIndex];
                    }
                    if (song != null)
                    {
                        if (this.logger.IsDebugEnabled)
                        {
                            this.logger.Debug("Getting url for song '{0}'.", song.SongId);
                        }

                        bool sameSong = false;
                        CancellationTokenSource source = null;
                        IRandomAccessStream stream = null;

                        await this.semaphore.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

                        try
                        {
                            if (this.currentSong != null && string.Equals(this.currentSong.SongId, song.SongId, StringComparison.OrdinalIgnoreCase))
                            {
                                sameSong = true;
                                stream = this.currentSongStream;
                                source = this.currentTokenSource;
                            }
                            else
                            {
                                if (this.currentTokenSource != null)
                                {
                                    this.currentTokenSource.Cancel();
                                    this.currentTokenSource = null;
                                }

                                if (this.predownloadTokenSource != null)
                                {
                                    if (nextSong)
                                    {
                                        source = this.predownloadTokenSource;
                                    }
                                    else
                                    {
                                        this.predownloadTokenSource.Cancel();
                                    }

                                    this.predownloadTokenSource = null;
                                }

                                if (source == null)
                                {
                                    source = new CancellationTokenSource();
                                }

                                this.currentTokenSource = source;
                            }
                        }
                        finally
                        {
                            this.semaphore.Release(1);
                        }

                        if (source.IsCancellationRequested)
                        {
                            return false;
                        }

                        // if null - playing the same stream
                        if (!sameSong)
                        {
                            stream = await this.songsCachingService.GetStreamAsync(song, source.Token);
                        }

                        if (stream != null && !source.IsCancellationRequested)
                        {
                            if (!sameSong)
                            {
                                await
                                    this.semaphore.WaitAsync(source.Token)
                                        .ConfigureAwait(continueOnCapturedContext: false);

                                if (source.IsCancellationRequested)
                                {
                                    return false;
                                }

                                try
                                {
                                    if (this.currentSongStream != null)
                                    {
                                        this.logger.Debug("Current song is not null. Disposing current stream.");

                                        var previousStream = this.currentSongStream as INetworkRandomAccessStream;
                                        if (previousStream != null)
                                        {
                                            previousStream.DownloadProgressChanged -=
                                                this.CurrentSongStreamOnDownloadProgressChanged;
                                        }

                                        this.currentSongStream.Dispose();
                                    }

                                    this.currentSong = song;
                                    this.currentSongStream = stream;

                                    var networkRandomAccessStream = stream as INetworkRandomAccessStream;
                                    if (networkRandomAccessStream != null && !networkRandomAccessStream.IsReady)
                                    {
                                        networkRandomAccessStream.DownloadProgressChanged +=
                                            this.CurrentSongStreamOnDownloadProgressChanged;
                                    }
                                    else
                                    {
                                        this.PredownloadNextSong();
                                    }
                                }
                                finally
                                {
                                    this.semaphore.Release(1);
                                }
                            }

                            if (source.IsCancellationRequested)
                            {
                                return false;
                            }

                            await this.mediaElement.PlayAsync(stream, "audio/mpeg");

                            if (source.IsCancellationRequested)
                            {
                                return false;
                            }

                            this.State = QueueState.Play;

                            this.logger.LogTask(this.publisherService.PublishAsync(song, this.CurrentPlaylist));

                            if (!(stream is INetworkRandomAccessStream))
                            {
                                this.RaiseDownloadProgress(1);
                            }

                            if (this.IsRadio && !this.CanSwitchToNext())
                            {
                                try
                                {
                                    var newRadioSongs =
                                        await
                                            this.radioStationsService.GetRadioSongsAsync(
                                                this.CurrentPlaylist.Id,
                                                this.songsQueue);
                                    await this.AddRangeAsync(this.CurrentPlaylist, newRadioSongs, false);
                                    this.eventAggregator.Publish(
                                        PlaylistsChangeEvent.New(PlaylistType.Radio)
                                            .AddUpdatedPlaylists(this.CurrentPlaylist));
                                }
                                catch (Exception e)
                                {
                                    this.logger.Error(e, "Cannot fetch next songs for radio");
                                }
                            }
                        }
                        else
                        {
                            this.State = QueueState.Stopped;

                            if (this.logger.IsDebugEnabled)
                            {
                                this.logger.Debug("Could not get url for song {0}.", song.SongId);
                            }
                        }
                    }
                }
                else
                {
                    this.State = QueueState.Stopped;
                }
            }
            catch (OperationCanceledException exception)
            {
                this.logger.Debug(exception, "Play song has been cancelled");
                return false;
            }
            catch (Exception exception)
            {
                this.logger.Error(exception, "Could not play song");
                return false;
            }

            return true;
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

            this.semaphore.Wait();

            try
            {
                if (this.predownloadTokenSource != null)
                {
                    this.predownloadTokenSource.Cancel();
                    this.predownloadTokenSource = null;
                }
            }
            finally
            {
                this.semaphore.Release(1);
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

            this.semaphore.Wait();

            try
            {
                if (this.currentSongStream != null)
                {
                    var networkRandomAccessStream = this.currentSongStream as INetworkRandomAccessStream;
                    if (networkRandomAccessStream == null || networkRandomAccessStream.IsReady)
                    {
                        this.PredownloadNextSong();
                    }
                }
            }
            finally
            {
                this.semaphore.Release(1);
            }
        }

        private void CurrentSongStreamOnDownloadProgressChanged(object sender, double e)
        {
            this.RaiseDownloadProgress(e);

            var networkRandomAccessStream = this.currentSongStream as INetworkRandomAccessStream;
            if (Math.Abs(1 - e) < 0.0001 && networkRandomAccessStream != null && networkRandomAccessStream.IsReady)
            {
                this.PredownloadNextSong();
            }
        }

        private async void PredownloadNextSong()
        {
            if ((this.currentQueueIndex + 1) < this.queueOrder.Count)
            {
                int nextIndex = this.currentQueueIndex + 1;
                var nextSong = this.songsQueue[this.queueOrder[nextIndex]];

                CancellationTokenSource source = new CancellationTokenSource();

                await this.semaphore.WaitAsync(source.Token).ConfigureAwait(continueOnCapturedContext: false);

                if (source.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    if (this.predownloadTokenSource != null)
                    {
                        this.predownloadTokenSource.Cancel();
                    }

                    this.predownloadTokenSource = source;
                }
                finally
                {
                    this.semaphore.Release(1);
                }

                if (source.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    await this.songsCachingService.PredownloadStreamAsync(nextSong, source.Token);
                }
                catch (OperationCanceledException exception)
                {
                    this.logger.Debug(exception, "Predownload stream cancelled");
                }
            }
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
            this.eventAggregator.Publish(new QueueChangeEvent(this.IsShuffled, this.Repeat, this.IsRadio, this.songsQueue));

            var handler = this.QueueChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private async Task ClearQueueAsync()
        {
            await this.StopAsync();
            this.queueOrder.Clear();
            this.songsQueue.Clear();
            this.currentQueueIndex = -1;
            this.currentSongStream = null;
            this.currentSong = null;
            this.CurrentPlaylist = null;
            this.RaiseQueueChanged();
        }
    }
}