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
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.WebServices;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    using Windows.Media;
    using Windows.System.Display;

    public class PlayerViewPresenter : ViewPresenterBase<IPlayerView>, ICurrentPlaylistService, IDisposable
    {
        private readonly ISongWebService songWebService;
        private readonly List<int> playOrder = new List<int>();
        private int playIndex = 0;

        private DisplayRequest request;

        public PlayerViewPresenter(
            IDependencyResolverContainer container, 
            IPlayerView view,
            ISongWebService songWebService)
            : base(container, view)
        {
            this.songWebService = songWebService;
            this.BindingModel = new PlayerBindingModel();

            MediaControl.PlayPauseTogglePressed += this.MediaControlPlayPauseTogglePressed;
            MediaControl.PlayPressed += this.MediaControlPlayPressed;
            MediaControl.PausePressed += this.MediaControlPausePressed;
            MediaControl.StopPressed += this.MediaControlStopPressed;

            this.BindingModel.SkipBackCommand = new DelegateCommand(this.PreviousSong, () => this.playIndex > 0);
            this.BindingModel.PlayCommand = new DelegateCommand(this.Play, () => !this.BindingModel.IsPlaying && this.BindingModel.Songs.Count > 0);
            this.BindingModel.PauseCommand = new DelegateCommand(this.Pause, () => this.BindingModel.IsPlaying);
            this.BindingModel.SkipAheadCommand = new DelegateCommand(
                                                            this.NextSong,
                                                            () => this.playIndex < (this.playOrder.Count - 1)
                                                                || this.BindingModel.IsRepeatAllEnabled && this.BindingModel.Songs.Count > 0);

            this.BindingModel.LockScreenCommand = new DelegateCommand(() =>
                {
                    if (this.request == null)
                    {
                        this.request = new DisplayRequest();
                        this.request.RequestActive();
                    }
                    else
                    {
                        this.request.RequestRelease();
                        this.request = null;
                    }

                    this.BindingModel.IsLockScreenEnabled = this.request != null; 
                });

            this.BindingModel.RepeatAllCommand = new DelegateCommand(() => this.BindingModel.IsRepeatAllEnabled = !this.BindingModel.IsRepeatAllEnabled);
            this.BindingModel.ShuffleCommand = new DelegateCommand(() =>
                { 
                    this.BindingModel.IsShuffleEnabled = !this.BindingModel.IsShuffleEnabled;
                    this.UpdateOrder();
                });

            this.BindingModel.UpdateBindingModel();
        }

        ~PlayerViewPresenter()
        {
            this.Dispose(disposing: false);
        }

        public PlayerBindingModel BindingModel { get; private set; }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        
        public void ClearPlaylist()
        {
            this.playIndex = -1;
            this.BindingModel.CurrentSongIndex = -1;
            this.BindingModel.Songs.Clear();
            this.UpdateOrder();
        }

        public void AddSongs(IEnumerable<GoogleMusicSong> songs)
        {
            if (songs == null)
            {
                throw new ArgumentNullException("songs");
            }

            foreach (var song in songs)
            {
                this.BindingModel.Songs.Add(new SongBindingModel(song));
            }

            this.UpdateOrder();
        }

        public void PlaySongs(IEnumerable<GoogleMusicSong> songs)
        {
            if (songs == null)
            {
                throw new ArgumentNullException("songs");
            }

            this.ClearPlaylist();
            this.AddSongs(songs);

            this.Dispatcher.RunAsync(
                () =>
                    {
                        if (this.BindingModel.Songs.Count > 0)
                        {
                            this.playIndex = 0;
                            this.BindingModel.CurrentSongIndex = this.playOrder[this.playIndex];
                            this.PlayCurrentSong();
                        }
                    });
        }

        public void OnMediaEnded()
        {
            this.BindingModel.State = PlayState.Stop;
            if (this.BindingModel.SkipAheadCommand.CanExecute())
            {
                this.NextSong();
            }
        }

        public void NextSong()
        {
            if (this.playIndex == (this.playOrder.Count - 1)
                && this.BindingModel.IsRepeatAllEnabled)
            {
                this.playIndex = 0;
                this.BindingModel.CurrentSongIndex = this.playOrder[this.playIndex];
            }
            else
            {
                this.playIndex++;
                this.BindingModel.CurrentSongIndex = this.playOrder[this.playIndex];
            }

            this.PlayCurrentSong();
            this.BindingModel.UpdateBindingModel();
        }

        public void PreviousSong()
        {
            if (this.playIndex != 0)
            {
                this.playIndex--;
                this.BindingModel.CurrentSongIndex = this.playOrder[this.playIndex];
            }

            this.PlayCurrentSong();

            this.BindingModel.UpdateBindingModel();
        }

        public void Play()
        {
            if (this.BindingModel.State == PlayState.Stop)
            {
                this.PlayCurrentSong();
            }
            else
            {
                this.View.Play();
                this.BindingModel.State = PlayState.Play;
            }

            this.BindingModel.UpdateBindingModel();
        }

        public void Pause()
        {
            this.View.Pause();
            this.BindingModel.State = PlayState.Pause;

            this.BindingModel.UpdateBindingModel();
        }

        public void Stop()
        {
            this.View.Stop();
            this.BindingModel.State = PlayState.Stop;

            this.BindingModel.UpdateBindingModel();
        }

        private void PlayCurrentSong()
        {
            this.BindingModel.UpdateBindingModel();

            var songBindingModel = this.BindingModel.CurrentSong;
            if (songBindingModel != null)
            {
                var song = songBindingModel.GetSong();
                this.songWebService.GetSongUrlAsync(song.Id).ContinueWith(
                    (t) =>
                        {
                            if (t.Result != null)
                            {
                                MediaControl.NextTrackPressed -= this.MediaControlOnNextTrackPressed;
                                MediaControl.NextTrackPressed -= this.MediaControlOnPreviousTrackPressed;

                                MediaControl.ArtistName = song.Artist;
                                MediaControl.TrackName = song.Title;

                                /*if (song.AlbumArtUrl != null)
                                {
                                    MediaControl.AlbumArt = new Uri("https:" + song.AlbumArtUrl);
                                }*/

                                this.View.PlaySong(new Uri(t.Result.Url));

                                if (this.BindingModel.SkipAheadCommand.CanExecute())
                                {
                                    MediaControl.NextTrackPressed += this.MediaControlOnNextTrackPressed;
                                }

                                if (this.BindingModel.SkipBackCommand.CanExecute())
                                {
                                    MediaControl.PreviousTrackPressed += this.MediaControlOnPreviousTrackPressed;
                                }

                                this.BindingModel.State = PlayState.Play;
                                this.BindingModel.UpdateBindingModel();
                            }
                            else
                            {
                                // TODO: Show error
                            }
                        }, 
                        TaskScheduler.FromCurrentSynchronizationContext());
            }
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
            this.Pause();
        }

        private void MediaControlPlayPressed(object sender, object e)
        {
            this.Play();
        }

        private void MediaControlStopPressed(object sender, object e)
        {
            this.Stop();
        }

        private void MediaControlPlayPauseTogglePressed(object sender, object e)
        {
            if (this.BindingModel.State == PlayState.Play)
            {
                this.Pause();
            }
            else
            {
                this.Play();
            }
        }

        private void MediaControlOnNextTrackPressed(object sender, object o)
        {
            this.NextSong();
        }

        private void MediaControlOnPreviousTrackPressed(object sender, object o)
        {
            this.PreviousSong();
        }

        private void UpdateOrder()
        {
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
                                if (x == this.BindingModel.CurrentSongIndex)
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

                this.playIndex = this.playOrder.IndexOf(this.BindingModel.CurrentSongIndex);
            }
        }
    }
}