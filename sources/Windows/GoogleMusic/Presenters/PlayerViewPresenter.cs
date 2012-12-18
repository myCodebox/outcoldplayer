// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.WebServices;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    using Windows.Media;

    public class PlayerViewPresenter : ViewPresenterBase<IPlayerView>, ICurrentPlaylistService, IDisposable
    {
        private readonly ISongWebService songWebService;

        private bool isPaused = false;

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
            this.BindingModel.CurrentSongIndex = -1;
            this.BindingModel.Songs.Clear();
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
        }

        public void PlaySongs(IEnumerable<GoogleMusicSong> songs)
        {
            if (songs == null)
            {
                throw new ArgumentNullException("songs");
            }

            this.ClearPlaylist();
            this.AddSongs(songs);

            if (this.BindingModel.Songs.Count > 0)
            {
                this.BindingModel.CurrentSongIndex = 0;
                this.PlayCurrentSong();
            }
        }

        public void OnMediaEnded()
        {
            this.NextSong();
        }

        private void PlayCurrentSong()
        {
            var songBindingModel = this.BindingModel.CurrentSong;
            if (songBindingModel != null)
            {
                var song = songBindingModel.GetSong();
                this.songWebService.GetSongUrlAsync(song.Id).ContinueWith(
                    (t) =>
                        {
                            if (t.Result != null)
                            {
                                this.isPaused = false;

                                MediaControl.NextTrackPressed -= this.MediaControlOnNextTrackPressed;
                                MediaControl.NextTrackPressed -= this.MediaControlOnPreviousTrackPressed;

                                MediaControl.ArtistName = song.Artist;
                                MediaControl.TrackName = song.Title;

                                /*if (song.AlbumArtUrl != null)
                                {
                                    MediaControl.AlbumArt = new Uri("https:" + song.AlbumArtUrl);
                                }*/

                                this.View.PlaySong(new Uri(t.Result.Url));

                                if (this.BindingModel.CurrentSongIndex < (this.BindingModel.Songs.Count - 1))
                                {
                                    MediaControl.NextTrackPressed += MediaControlOnNextTrackPressed;
                                }

                                if (this.BindingModel.CurrentSongIndex != 0)
                                {
                                    MediaControl.PreviousTrackPressed += MediaControlOnPreviousTrackPressed;
                                }
                            }
                            else
                            {
                                // TODO: Show error
                            }
                        });
            }
        }

        private void MediaControlPausePressed(object sender, object e)
        {
            this.View.Pause();
            this.isPaused = true;
            // this.View.Stop();
        }

        private void MediaControlPlayPressed(object sender, object e)
        {
            if (this.isPaused)
            {
                this.View.Play();
                this.isPaused = false;
            }
            else
            {
                this.PlayCurrentSong();
            }
            // this.PlayCurrentSong();
        }

        private void MediaControlStopPressed(object sender, object e)
        {
            this.View.Stop();
            this.isPaused = false;
        }

        private void MediaControlPlayPauseTogglePressed(object sender, object e)
        {
            if (this.isPaused)
            {
                this.View.Play();
                // this.PlayCurrentSong();
                this.isPaused = false;
            }
            else
            {
                //this.View.Stop();
                this.View.Pause();
                this.isPaused = true;
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

        private void NextSong()
        {
            this.BindingModel.CurrentSongIndex++;
            this.PlayCurrentSong();
        }

        private void PreviousSong()
        {
            this.BindingModel.CurrentSongIndex--;
            this.PlayCurrentSong();
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
    }
}