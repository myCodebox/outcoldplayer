// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Services.Publishers;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.Presenters;

    using Windows.Media;
    using Windows.System.Display;
    using Windows.UI.Popups;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public class PlayerViewPresenter : ViewPresenterBase<IPlayerView>, ICurrentPlaylistService, IDisposable
    {
        private readonly DispatcherTimer timer = new DispatcherTimer();

        private readonly ISongWebService songWebService;
        private readonly IGoogleMusicSessionService sessionService;
        private readonly ISettingsService settingsService;

        private readonly IMediaStreamDownloadService mediaStreamDownloadService;

        private readonly ICurrentSongPublisherService publisherService;

        private readonly List<int> playOrder = new List<int>();

        private MediaElement mediaElement;

        private int playIndex = 0;

        private DisplayRequest request;

        private INetworkRandomAccessStream currentSongStream;

        private Playlist currentPlaylist;

        public PlayerViewPresenter(
            MediaElement mediaElement,
            ISongWebService songWebService,
            IGoogleMusicSessionService sessionService,
            ISettingsService settingsService,
            IMediaStreamDownloadService mediaStreamDownloadService,
            ICurrentSongPublisherService publisherService)
        {
            this.mediaElement = mediaElement;
            this.songWebService = songWebService;
            this.sessionService = sessionService;
            this.settingsService = settingsService;
            this.mediaStreamDownloadService = mediaStreamDownloadService;
            this.publisherService = publisherService;
            this.BindingModel = new PlayerBindingModel
                                    {
                                        IsRepeatAllEnabled =
                                            this.settingsService.GetValue("IsRepeatAllEnabled", defaultValue: false),
                                        IsShuffleEnabled =
                                            this.settingsService.GetValue("IsShuffleEnabled", defaultValue: false),
                                        IsLockScreenEnabled =
                                            this.settingsService.GetValue("IsLockScreenEnabled", defaultValue: false)
                                    };

            if (this.BindingModel.IsLockScreenEnabled)
            {
                this.UpdateLockScreen();
            }

            MediaControl.PlayPauseTogglePressed += this.MediaControlPlayPauseTogglePressed;
            MediaControl.PlayPressed += this.MediaControlPlayPressed;
            MediaControl.PausePressed += this.MediaControlPausePressed;
            MediaControl.StopPressed += this.MediaControlStopPressed;

            this.BindingModel.SkipBackCommand = new DelegateCommand(this.PreviousSong, () => !this.BindingModel.IsBusy && this.playIndex > 0);
            this.BindingModel.PlayCommand = new DelegateCommand(this.Play, () => !this.BindingModel.IsBusy && !this.BindingModel.IsPlaying && this.BindingModel.Songs.Count > 0);
            this.BindingModel.PauseCommand = new DelegateCommand(this.Pause, () => !this.BindingModel.IsBusy && this.BindingModel.IsPlaying);
            this.BindingModel.SkipAheadCommand = new DelegateCommand(
                this.NextSong,
                () =>
                !this.BindingModel.IsBusy
                && ((this.playIndex < (this.playOrder.Count - 1))
                    || (this.BindingModel.IsRepeatAllEnabled && this.BindingModel.Songs.Count > 0)));

            this.BindingModel.LockScreenCommand = new DelegateCommand(this.UpdateLockScreen);

            this.BindingModel.RepeatAllCommand = new DelegateCommand(() => this.BindingModel.IsRepeatAllEnabled = !this.BindingModel.IsRepeatAllEnabled);
            this.BindingModel.ShuffleCommand = new DelegateCommand(() =>
                { 
                    this.BindingModel.IsShuffleEnabled = !this.BindingModel.IsShuffleEnabled;
                    this.UpdateOrder();
                });

            this.BindingModel.UpdateBindingModel();

            this.timer.Interval = TimeSpan.FromMilliseconds(500);
            this.timer.Start();

            this.BindingModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName.Equals("CurrentPosition"))
                    {
                        if (Math.Abs(this.mediaElement.Position.TotalSeconds - this.BindingModel.CurrentPosition) > 2)
                        {
                            this.mediaElement.Position = TimeSpan.FromSeconds(this.BindingModel.CurrentPosition);
                        }
                    }
                    else if (args.PropertyName.Equals("IsRepeatAllEnabled"))
                    {
                        this.settingsService.SetValue("IsRepeatAllEnabled", this.BindingModel.IsRepeatAllEnabled);
                    }
                    else if (args.PropertyName.Equals("IsShuffleEnabled"))
                    {
                        this.settingsService.SetValue("IsShuffleEnabled", this.BindingModel.IsShuffleEnabled);
                    }
                    else if (args.PropertyName.Equals("IsLockScreenEnabled"))
                    {
                        this.settingsService.SetValue("IsLockScreenEnabled", this.BindingModel.IsLockScreenEnabled);
                    }
                };
           
            this.timer.Tick += (sender, o) =>
            {
                if (this.BindingModel.IsPlaying)
                {
                    this.BindingModel.CurrentPosition = this.mediaElement.Position.TotalSeconds;
                }
            };

            this.sessionService.SessionCleared += (sender, args) => this.Dispatcher.RunAsync(
                () =>
                    {
                        this.Stop();
                        this.ClearPlaylist();
                    });
        }

        ~PlayerViewPresenter()
        {
            this.Dispose(disposing: false);
        }

        public event EventHandler PlaylistChanged;

        public PlayerBindingModel BindingModel { get; private set; }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void ClearPlaylist()
        {
            this.Logger.Debug("ClearPlaylist.");
            this.playIndex = -1;
            this.BindingModel.CurrentSongIndex = -1;
            this.BindingModel.Songs.Clear();
            this.UpdateOrder();
            this.BindingModel.UpdateBindingModel();

            this.RaisePlaylistChanged();
        }

        public void SetPlaylist(Playlist playlist)
        {
            this.currentPlaylist = playlist;
            this.AddSongs(playlist.Songs);
        }

        public void AddSongs(IEnumerable<Song> songs)
        {
            this.Logger.Debug("AddSongs.");

            if (songs == null)
            {
                throw new ArgumentNullException("songs");
            }

            foreach (var song in songs)
            {
                this.BindingModel.Songs.Add(song);
            }

            this.UpdateOrder();
            // TODO: this.Dispatcher.RunAsync(() => this.View.Activate());

            this.RaisePlaylistChanged();
        }
 
        public IEnumerable<Song> GetPlaylist()
        {
            return this.BindingModel.Songs;
        }

        public async Task PlayAsync(int songIndex = -1)
        {
            if (songIndex < 0 && this.playOrder.Count > 0)
            {
                songIndex = this.playOrder[0];
            }

            if (songIndex >= 0)
            {
                var songBindingModel = this.BindingModel.Songs[songIndex];
                if (songBindingModel == null)
                {
                    this.Logger.Error("Cannot find song with index '{0}'", songIndex);
                }
                else
                {
                    await this.Dispatcher.RunAsync(
                        () =>
                            {
                                this.playIndex = this.playOrder.IndexOf(songIndex);
                                this.PlayCurrentSong();
                            });

                    // TODO:  this.View.Activate();
                }
            }
        }

        public async Task RemoveAsync(int songIndex)
        {
            if (this.BindingModel.Songs.Count > songIndex)
            {
                var songBindingModel = this.BindingModel.Songs[songIndex];
                if (songBindingModel == null)
                {
                    this.Logger.Error("Cannot find song with index '{0}'", songIndex);
                }
                else
                {
                    await this.Dispatcher.RunAsync(
                        () =>
                            {
                                var index = this.playOrder.IndexOf(songIndex);

                                this.playOrder.RemoveAt(index);
                                for (int i = 0; i < this.playOrder.Count; i++)
                                {
                                    if (this.playOrder[i] > songIndex)
                                    {
                                        this.playOrder[i]--;
                                    }
                                }

                                this.BindingModel.Songs.Remove(songBindingModel);

                                if (index == this.playIndex)
                                {
                                    this.Stop();
                                    this.PlayCurrentSong();
                                }
                                else 
                                {
                                    if (index < this.playIndex)
                                    {
                                        this.playIndex--;
                                    }
                                }
                                
                                if (this.BindingModel.Songs.Count == 0)
                                {
                                    this.ClearPlaylist();
                                }
                                else
                                {
                                    this.RaisePlaylistChanged();
                                    this.BindingModel.UpdateBindingModel();
                                }
                            });
                }
            }
        }

        public int CurrentSongIndex
        {
            get
            {
                return this.BindingModel.CurrentSongIndex;
            }
        }
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            this.mediaElement.MediaOpened += (sender, args) =>
            {
                this.Logger.Info("Media opened. Duration: {0}.", this.mediaElement.NaturalDuration.TimeSpan);

                this.BindingModel.CurrentPosition = 0;
                this.BindingModel.TotalSeconds = this.mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            };

            this.mediaElement.MediaEnded += (sender, args) =>
            {
                this.Logger.Info("Media Ended");
                this.OnMediaEnded();
            };

            this.mediaElement.MediaFailed += (sender, args) =>
            {
                this.Logger.Error("Media Failed: {0}", args.ErrorMessage);
                this.Logger.Debug("Media Failed - trying to handle this like MediaEnded");
                this.OnMediaEnded();
            };
        }

        private void NextSong()
        {
            this.Logger.Debug("NextSong.");

            if (this.playIndex == (this.playOrder.Count - 1)
                && this.BindingModel.IsRepeatAllEnabled)
            {
                this.playIndex = 0;
            }
            else
            {
                this.playIndex++;
            }

            this.PlayCurrentSong();
            this.BindingModel.UpdateBindingModel();
        }

        private void PreviousSong()
        {
            this.Logger.Debug("PreviousSong.");

            if (this.playIndex != 0)
            {
                this.playIndex--;
            }

            this.PlayCurrentSong();

            this.BindingModel.UpdateBindingModel();
        }

        private void Play()
        {
            this.Logger.Debug("Play.");

            if (this.BindingModel.State == PlayState.Stop)
            {
                this.PlayCurrentSong();
            }
            else
            {
                this.publisherService.PublishAsync(this.BindingModel.CurrentSong, this.currentPlaylist);

                this.mediaElement.Play();
                this.BindingModel.State = PlayState.Play;
            }

            this.BindingModel.UpdateBindingModel();
        }

        private void Pause()
        {
            this.Logger.Debug("Pause.");

            this.mediaElement.Pause();
            this.BindingModel.State = PlayState.Pause;
            this.publisherService.CancelActiveTasks();

            this.BindingModel.UpdateBindingModel();
        }

        private void Stop()
        {
            this.Logger.Debug("Stop.");

            this.publisherService.CancelActiveTasks();
            this.mediaElement.Stop();
            this.BindingModel.State = PlayState.Stop;

            this.BindingModel.UpdateBindingModel();
        }

        private void PlayCurrentSong()
        {
            this.Logger.Debug("PlayCurrentSong.");

            this.BindingModel.UpdateBindingModel();
            this.publisherService.CancelActiveTasks();

            this.Stop();

            if (this.playOrder.Count > this.playIndex)
            {
                var currentSongIndex = this.playOrder[this.playIndex];
                var songBindingModel = this.BindingModel.Songs[currentSongIndex];
                if (songBindingModel != null)
                {
                    this.Logger.Debug("Found current song.");
                    var song = songBindingModel;

                    this.Logger.Debug("Getting url for song '{0}'.", song.Metadata.Id);

                    this.BindingModel.IsBusy = true;
                    this.songWebService.GetSongUrlAsync(song.Metadata.Id).ContinueWith(
                        async (t) =>
                            {
                                if (t.IsCompleted && !t.IsFaulted && t.Result != null)
                                {
                                    if (this.currentSongStream != null)
                                    {
                                        this.Logger.Debug("Current song is not null .Stopping medial element.");

                                        await this.Dispatcher.RunAsync(() => this.mediaElement.Stop());

                                        this.Logger.Debug("Disposing current song.");
                                        this.currentSongStream.DownloadProgressChanged -= CurrentSongStreamOnDownloadProgressChanged;
                                        this.currentSongStream.Dispose();
                                        this.currentSongStream = null;
                                    }

                                    this.Logger.Debug("Request completed. Trying to get stream by url '{0}'.", t.Result.Url);
                                    var stream = await mediaStreamDownloadService.GetStreamAsync(t.Result.Url);

                                    await this.Dispatcher.RunAsync(
                                        () =>
                                            {
                                                this.Logger.Debug("Setting current song.");
                                                this.currentSongStream = stream;
                                                this.currentSongStream.DownloadProgressChanged += CurrentSongStreamOnDownloadProgressChanged;

                                                this.BindingModel.CurrentSongIndex = currentSongIndex;

                                                this.Logger.Info("Set new source for media element with content type '{0}'.", this.currentSongStream.ContentType);

                                                this.mediaElement.SetSource(this.currentSongStream, this.currentSongStream.ContentType);
                                                this.mediaElement.Play();

                                                this.BindingModel.State = PlayState.Play;
                                                this.BindingModel.UpdateBindingModel();

                                                this.BindingModel.IsBusy = false;

                                                this.UpdateMediaButtons();

                                                this.publisherService.PublishAsync(this.BindingModel.CurrentSong, this.currentPlaylist);
                                            });
                                }
                                else
                                {
                                    await this.Dispatcher.RunAsync(
                                        () =>
                                            {
                                                this.BindingModel.IsBusy = false;
                                                this.Logger.Debug(
                                                    "Could not find url for song {0}.", song.Metadata.Id);
                                                var tResult = (new MessageDialog(
                                                    "Cannot play right now. Make sure that you don't use current account on different device at the same time. Try after couple minutes."))
                                                    .ShowAsync();
                                            });
                                }
                            });
                }
            }
        }

        private void UpdateMediaButtons()
        {
            if (this.BindingModel.SkipAheadCommand.CanExecute())
            {
                MediaControl.NextTrackPressed -= this.MediaControlOnNextTrackPressed;
                MediaControl.NextTrackPressed += this.MediaControlOnNextTrackPressed;
            }
            else
            {
                MediaControl.NextTrackPressed -= this.MediaControlOnNextTrackPressed;
            }

            if (this.BindingModel.SkipBackCommand.CanExecute())
            {
                MediaControl.PreviousTrackPressed -= this.MediaControlOnPreviousTrackPressed;
                MediaControl.PreviousTrackPressed += this.MediaControlOnPreviousTrackPressed;
            }
            else
            {
                MediaControl.PreviousTrackPressed -= this.MediaControlOnPreviousTrackPressed;
            }
        }

        private void CurrentSongStreamOnDownloadProgressChanged(object sender, double d)
        {
            this.Dispatcher.RunAsync(
                () =>
                    {
                        this.Logger.Info("Download progress changed to {0}", d);
                        this.BindingModel.DownloadProgress = d;
                    });
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                MediaControl.PlayPauseTogglePressed -= this.MediaControlPlayPauseTogglePressed;
                MediaControl.PlayPressed -= this.MediaControlPlayPressed;
                MediaControl.PausePressed -= this.MediaControlPausePressed;
                MediaControl.StopPressed -= this.MediaControlStopPressed;
            }
        }

        private void MediaControlPausePressed(object sender, object e)
        {
            this.Logger.Debug("MediaControlPausePressed.");
            this.Dispatcher.RunAsync(this.Pause);
        }

        private void MediaControlPlayPressed(object sender, object e)
        {
            this.Logger.Debug("MediaControlPlayPressed.");
            this.Dispatcher.RunAsync(this.Play);
        }

        private void MediaControlStopPressed(object sender, object e)
        {
            this.Logger.Debug("MediaControlStopPressed.");
            this.Dispatcher.RunAsync(this.Stop);
        }

        private void MediaControlPlayPauseTogglePressed(object sender, object e)
        {
            this.Logger.Debug("MediaControlPlayPauseTogglePressed.");
            if (this.BindingModel.State == PlayState.Play)
            {
                this.Dispatcher.RunAsync(this.Pause);
            }
            else
            {
                this.Dispatcher.RunAsync(this.Play);
            }
        }

        private void MediaControlOnNextTrackPressed(object sender, object o)
        {
            this.Logger.Debug("MediaControlOnNextTrackPressed.");
            this.Dispatcher.RunAsync(this.NextSong);
        }

        private void MediaControlOnPreviousTrackPressed(object sender, object o)
        {
            this.Logger.Debug("MediaControlOnPreviousTrackPressed.");
            this.Dispatcher.RunAsync(this.PreviousSong);
        }

        private void UpdateOrder()
        {
            this.Logger.Debug("UpdateOrder.");
            this.playOrder.Clear();

            if (this.BindingModel.Songs.Count > 0)
            {
                var range = Enumerable.Range(0, this.BindingModel.Songs.Count);

                if (this.BindingModel.IsShuffleEnabled)
                {
                    var random = new Random((int)DateTime.Now.Ticks);
                    this.playOrder.AddRange(
                        range.ToDictionary(
                            x =>
                            {
                                if (this.BindingModel.CurrentSong != null 
                                    && x == this.BindingModel.CurrentSongIndex)
                                {
                                    return -1;
                                }
                                else
                                {
                                    return random.Next();
                                }
                            }, 
                            x => x)
                            .OrderBy(x => x.Key).Select(x => x.Value));
                }
                else
                {
                    this.playOrder.AddRange(range);
                }

                if (this.Logger.IsInfoEnabled)
                {
                    this.Logger.Info("Shuffle enabled: {0}", this.BindingModel.IsShuffleEnabled);
                    this.Logger.Info("Playing order: {0}", string.Join(",", this.playOrder));
                }

                if (this.BindingModel.CurrentSong != null)
                {
                    this.playIndex = this.playOrder.IndexOf(this.BindingModel.CurrentSongIndex);
                }
                else
                {
                    this.playIndex = 0;
                }
            }

            this.BindingModel.UpdateBindingModel();
            this.UpdateMediaButtons();
        }

        private void OnMediaEnded()
        {
            this.Logger.Debug("OnMediaEnded.");

            this.BindingModel.State = PlayState.Stop;
            if (this.BindingModel.SkipAheadCommand.CanExecute())
            {
                this.NextSong();
            }
        }

        private void RaisePlaylistChanged()
        {
            var handler = this.PlaylistChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void UpdateLockScreen()
        {
            if (this.request == null)
            {
                this.request = new DisplayRequest();
                this.request.RequestActive();
                this.Logger.Debug("Request display active.");
            }
            else
            {
                this.request.RequestRelease();
                this.request = null;
                this.Logger.Debug("Release display active.");
            }

            this.BindingModel.IsLockScreenEnabled = this.request != null;
        }
    }
}