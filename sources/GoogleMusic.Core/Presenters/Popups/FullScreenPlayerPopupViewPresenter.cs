// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class FullScreenPlayerPopupViewPresenter : DisposableViewPresenterBase<IFullScreenPlayerPopupView>
    {
        private readonly Random random = new Random((int) DateTime.Now.Ticks);
        private readonly IPlayQueueService playQueueService;
        private readonly PlayerViewPresenter playerViewPresenter;
        private ITimer timer;
        private Uri currentSongAlbumArt;
        private Uri currentSongArtistArt;
        private IList<Uri> queueAlbumArts;

        private IList<int> notUpdatedAlbumArts; 

        public FullScreenPlayerPopupViewPresenter(
            IPlayQueueService playQueueService,
            PlayerViewPresenter playerViewPresenter)
        {
            this.playQueueService = playQueueService;
            this.playerViewPresenter = playerViewPresenter;
        }

        public PlayerViewPresenter PlayerViewPresenter
        {
            get { return this.playerViewPresenter; }
        }

        public Uri CurrentSongAlbumArt
        {
            get { return this.currentSongAlbumArt; }
            set { this.SetValue(ref this.currentSongAlbumArt, value); }
        }

        public Uri CurrentSongArtistArt
        {
            get { return this.currentSongArtistArt; }
            set { this.SetValue(ref this.currentSongArtistArt, value); }
        }

        public Uri AlbumArt0
        {
            get { return this.GetAlbumArt(); }
        }

        public Uri AlbumArt1
        {
            get { return this.GetAlbumArt(); }
        }

        public Uri AlbumArt2
        {
            get { return this.GetAlbumArt(); }
        }

        public Uri AlbumArt3
        {
            get { return this.GetAlbumArt(); }
        }

        public Uri AlbumArt4
        {
            get { return this.GetAlbumArt(); }
        }

        public Uri AlbumArt5
        {
            get { return this.GetAlbumArt(); }
        }

        public Uri AlbumArt6
        {
            get { return this.GetAlbumArt(); }
        }

        public Uri AlbumArt7
        {
            get { return this.GetAlbumArt(); }
        }

        public Uri AlbumArt8
        {
            get { return this.GetAlbumArt(); }
        }

        public Uri AlbumArt9
        {
            get { return this.GetAlbumArt(); }
        }

        public Uri AlbumArt10
        {
            get { return this.GetAlbumArt(); }
        }

        public Uri AlbumArt11
        {
            get { return this.GetAlbumArt(); }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.playQueueService.StateChanged += this.PlayQueueServiceOnStateChanged;
            this.UpdateCurrentSongAlbumArt(playQueueService.GetCurrentSong());
            this.timer = ApplicationContext.Container.Resolve<ITimer>();
            this.timer.Tick += TimerOnTick;
            this.timer.Interval = new TimeSpan(0, 0, 3);
            this.timer.Start();

            this.RegisterForDispose(
                this.EventAggregator.GetEvent<SizeChangeEvent>()
                    .Where(x => x.IsSmall)
                    .Subscribe(async (x) => await this.Dispatcher.RunAsync(() => this.View.Close())));
        }

        protected override void OnDisposing()
        {
            base.OnDisposing();
            this.playQueueService.StateChanged -= this.PlayQueueServiceOnStateChanged;
            this.timer.Stop();
        }

        private void PlayQueueServiceOnStateChanged(object sender, StateChangedEventArgs eventArgs)
        {
           this.UpdateCurrentSongAlbumArt(eventArgs.CurrentSong);
        }

        private async void UpdateCurrentSongAlbumArt(Song currentSong)
        {
            await this.Dispatcher.RunAsync(() =>
            {
                Uri oldAlbumArt = this.CurrentSongAlbumArt;
                this.CurrentSongAlbumArt = currentSong == null ? null : currentSong.AlbumArtUrl;
                this.CurrentSongArtistArt = currentSong == null ? null : currentSong.ArtistArtUrl;

                this.queueAlbumArts = this.playQueueService.GetQueue()
                    .Where(x => x.AlbumArtUrl != this.CurrentSongAlbumArt)
                    .Select(x => x.AlbumArtUrl)
                    .ToList();

                if (oldAlbumArt == null)
                {
                    foreach (var index in Enumerable.Range(0, 12))
                    {
                        this.RaisePropertyChanged("AlbumArt" + index);
                    }
                }
            });
        }

        private Uri GetAlbumArt()
        {
            if (this.queueAlbumArts != null && this.queueAlbumArts.Count > 0)
            {
                if (this.queueAlbumArts.Count <= 2 && this.random.Next(0, 3) == 0)
                {
                    return null;
                }

                return this.queueAlbumArts[this.random.Next(0, this.queueAlbumArts.Count)];
            }

            return null;
        }

        private void TimerOnTick(object sender, object o)
        {
            if (this.notUpdatedAlbumArts == null || this.notUpdatedAlbumArts.Count == 0)
            {
                this.notUpdatedAlbumArts = new List<int>(Enumerable.Range(0, 12));
            }

            int index = this.random.Next(0, this.notUpdatedAlbumArts.Count);
            int albumArtIndex = this.notUpdatedAlbumArts[index];
            this.notUpdatedAlbumArts.RemoveAt(index);

            this.RaisePropertyChanged("AlbumArt" + albumArtIndex);
        }
    }
}
