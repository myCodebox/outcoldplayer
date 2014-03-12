// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Linq;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Controls;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.EventAggregator;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public enum SongsSorting
    {
        Unknown = 0,
        Track = 1,
        TrackDescending = 2,
        Title = 3,
        TitleDescending = 4,
        Artist = 5,
        ArtistDescending = 6,
        Album = 7,
        AlbumDescending = 8,
        Duration = 9,
        DurationDescending = 10,
        Rating = 11,
        RatingDescending = 12,
        PlaysCount = 13,
        PlaysCountDescending = 14
    }

    public class SongsListViewPresenter : ViewPresenterBase<ISongsListView>
    {
        private readonly IPlayQueueService playQueueService;

        private readonly IDispatcher dispatcher;

        private readonly ISongsService songsService;

        private readonly ISelectedObjectsService selectedObjectsService;

        private ObservableCollection<SongBindingModel> songs;
        private SongsSorting currentSorting = SongsSorting.Unknown;

        private readonly ILogger logger;

        private IPlaylist playlist;

        private IList<Song> collection;

        private int maxItems = int.MaxValue;

        private bool allowSorting = true;

        public SongsListViewPresenter(
            IPlayQueueService playQueueService,
            IEventAggregator eventAggregator,
            IDispatcher dispatcher,
            ISongsService songsService,
            ISelectedObjectsService selectedObjectsService,
            ILogManager logManager)
        {
            this.playQueueService = playQueueService;
            this.dispatcher = dispatcher;
            this.songsService = songsService;
            this.selectedObjectsService = selectedObjectsService;
            this.Songs = new ObservableCollection<SongBindingModel>();
            this.SortCommand = new DelegateCommand(this.SortSongs, (e) => this.AllowSorting);
            this.RateSongCommand = new DelegateCommand(this.RateSong);
            this.SelectedItems = new ObservableCollection<SongBindingModel>();

            this.logger = logManager.CreateLogger("SongsListViewPresenter");

            this.playQueueService.StateChanged += this.PlayQueueServiceOnStateChanged;

            eventAggregator.GetEvent<SongsUpdatedEvent>()
                           .Subscribe(async (e) => await this.dispatcher.RunAsync(() => this.OnSongsUpdated(e.UpdatedSongs)));

            eventAggregator.GetEvent<SongCachingChangeEvent>()
                           .Where(e => e.EventType == SongCachingChangeEventType.FinishDownloading || e.EventType == SongCachingChangeEventType.RemoveLocalCopy)
                           .Subscribe(async (e) => await this.dispatcher.RunAsync(() => this.UpdateIfSongCached(e.Song, e.EventType)));

            eventAggregator.GetEvent<CachingChangeEvent>()
                           .Where(e => e.EventType == SongCachingChangeEventType.ClearCache)
                           .Subscribe(async (e) => await this.dispatcher.RunAsync(this.ClearAllIsCachedIcons));

            eventAggregator.GetEvent<SelectionClearedEvent>()
                            .Subscribe(async (e) => await this.dispatcher.RunAsync(this.ClearSelectedItems));

            eventAggregator.GetEvent<SelectSongByIdEvent>()
                            .Subscribe(async (e) => await this.dispatcher.RunAsync(
                                () =>
                                {
                                    var songBindingModel = this.Songs.FirstOrDefault(s => string.Equals(s.Metadata.SongId, e.SongId, StringComparison.OrdinalIgnoreCase));
                                    if (songBindingModel != null)
                                    {
                                        this.SelectedItems.Add(songBindingModel);
                                    }
                                }));

            eventAggregator.GetEvent<SelectCurrentPlaylistSongEvent>()
                            .Subscribe(async (e) => await this.dispatcher.RunAsync(
                                () =>
                                {
                                    this.ClearSelectedItems();
                                    this.SelectSongByIndex(this.playQueueService.GetCurrentSongIndex());
                                }));

            this.SelectedItems.CollectionChanged += this.SelectionChanged;
        }

        public DelegateCommand SortCommand { get; set; }

        public DelegateCommand RateSongCommand { get; set; }

        public ObservableCollection<SongBindingModel> Songs
        {
            get
            {
                return this.songs;
            }

            private set
            {
                this.SetValue(ref this.songs, value);
            }
        }

        public IPlaylist ViewPlaylist
        {
            get
            {
                return this.playlist;
            }

            set
            {
                this.SetValue(ref this.playlist, value);
            }
        }

        public ObservableCollection<SongBindingModel> SelectedItems { get; set; }

        public SongsSorting CurrentSorting
        {
            get
            {
                return this.currentSorting;
            }

            set
            {
                this.SetValue(ref this.currentSorting, value);
            }
        }

        public int MaxItems
        {
            get
            {
                return this.maxItems;
            }

            set
            {
                this.maxItems = value;
            }
        }

        public bool AllowSorting
        {
            get
            {
                return this.allowSorting;
            }

            set
            {
                this.allowSorting = value;
                this.SortCommand.RaiseCanExecuteChanged();
            }
        }

        public void PlaySong(SongBindingModel songBindingModel)
        {
            if (songBindingModel != null)
            {
                int songIndex = this.Songs.IndexOf(songBindingModel);
                this.logger.LogTask(this.playQueueService.PlayAsync(this.ViewPlaylist, this.SortSongs(this.CurrentSorting), songIndex));
            }
        }

        public void SetCollection(IEnumerable<Song> collectionSongs)
        {
            this.collection = collectionSongs == null ? null : collectionSongs.ToList();

            if (this.collection == null)
            {
                this.Songs = null;
            }
            else
            {
                this.Songs = new ObservableCollection<SongBindingModel>(this.collection.Take(this.MaxItems).Select(s => new SongBindingModel(s)));
            }

            this.CurrentSorting = SongsSorting.Unknown;
            this.ClearSelectedItems();
            this.UpdateSongStates(this.playQueueService.GetCurrentSong(), this.playQueueService.State);
        }

        public IEnumerable<Song> GetSelectedSongs()
        {
            return this.SelectedItems.Select(bm => bm.Metadata).ToList();
        }

        public IEnumerable<int> GetSelectedIndexes()
        {
            return this.SelectedItems.Select(bm => this.Songs.IndexOf(bm)).ToList();
        }

        public void SelectSongByIndex(int index)
        {
            if (this.Songs == null || this.Songs.Count <= index)
            {
                return;
            }

            if (index == -1)
            {
                this.ClearSelectedItems();
            }
            else
            {
                SongBindingModel selectedSong = this.Songs[index];
                if (!this.SelectedItems.Contains(selectedSong))
                {
                    this.SelectedItems.Add(selectedSong);
                }
            }
        }

        public void ClearSelectedItems()
        {
            if (this.SelectedItems.Count > 0)
            {
                this.SelectedItems.Clear();
            }
        }

        private void OnSongsUpdated(IEnumerable<Song> updateSongs)
        {
            if (updateSongs != null && this.collection != null)
            {
                foreach (var updateSong in updateSongs)
                {
                    var songId = updateSong.SongId;
                    foreach (var songBindingModel in this.Songs.Where(s => string.Equals(s.Metadata.SongId, songId, StringComparison.Ordinal)))
                    {
                         songBindingModel.Metadata = updateSong;
                    }

                    foreach (var song in this.collection.Select((s, i) => new { Song = s, Index = i }).Where(x => string.Equals(x.Song.SongId, songId, StringComparison.Ordinal)).ToList())
                    {
                        this.collection[song.Index] = song.Song;
                    }
                }
            }
        }

        private void UpdateIfSongCached(Song song, SongCachingChangeEventType eventType)
        {
            if (song != null && this.Songs != null)
            {
                foreach (var songBindingModel in this.Songs.Where(s => string.Equals(s.Metadata.SongId, song.SongId, StringComparison.Ordinal)))
                {
                    songBindingModel.IsCached = eventType == SongCachingChangeEventType.FinishDownloading;
                }
            }
        }

        private void ClearAllIsCachedIcons()
        {
            if (this.Songs != null)
            {
                foreach (var songBindingModel in this.Songs.Where(s => s.IsCached))
                {
                    songBindingModel.IsCached = false;
                }
            }
        }

        private async void PlayQueueServiceOnStateChanged(object sender, StateChangedEventArgs stateChangedEventArgs)
        {
            await this.dispatcher.RunAsync(
                () => this.UpdateSongStates(stateChangedEventArgs.CurrentSong, stateChangedEventArgs.State));
        }

        private void UpdateSongStates(Song currentSong, QueueState queueState)
        {
            if (this.Songs != null)
            {
                foreach (var song in this.Songs)
                {
                    if (currentSong != null && string.Equals(song.Metadata.SongId, currentSong.SongId, StringComparison.Ordinal))
                    {
                        if (queueState == QueueState.Play)
                        {
                            song.State = SongState.Playing;
                        }
                        else if (queueState == QueueState.Paused)
                        {
                            song.State = SongState.Paused;
                        }
                        else if (queueState == QueueState.Busy)
                        {
                            song.State = SongState.Loading;
                        }
                        else
                        {
                            song.State = SongState.None;
                        }
                    }
                    else
                    {
                        song.State = SongState.None;
                    }
                }
            }
        }

        private void SortSongs(object obj)
        {
            if (obj != null && this.collection != null)
            {
                SongsSorting songsSorting = (SongsSorting)Enum.ToObject(typeof(SongsSorting), obj);
                IEnumerable<Song> enumerable = this.SortSongs(songsSorting);

                this.ClearSelectedItems();
                this.Songs = new ObservableCollection<SongBindingModel>(enumerable.Select(x => new SongBindingModel(x)));
                this.CurrentSorting = songsSorting;
                this.UpdateSongStates(this.playQueueService.GetCurrentSong(), this.playQueueService.State);
            }
        }

        private IEnumerable<Song> SortSongs(SongsSorting songsSorting)
        {
            IEnumerable<Song> enumerable;
            switch (songsSorting)
            {
                case SongsSorting.Unknown:
                    enumerable = this.collection;
                    break;
                case SongsSorting.Track:
                    enumerable = this.collection.OrderBy(s => s.Track);
                    break;
                case SongsSorting.TrackDescending:
                    enumerable = this.collection.OrderByDescending(s => s.Track);
                    break;
                case SongsSorting.Title:
                    enumerable = this.collection.OrderBy(s => s.Title, StringComparer.CurrentCultureIgnoreCase);
                    break;
                case SongsSorting.TitleDescending:
                    enumerable = this.collection.OrderByDescending(s => s.Title, StringComparer.CurrentCultureIgnoreCase);
                    break;
                case SongsSorting.Artist:
                    enumerable = this.collection.OrderBy(s => s.ArtistTitle, StringComparer.CurrentCultureIgnoreCase);
                    break;
                case SongsSorting.ArtistDescending:
                    enumerable = this.collection.OrderByDescending(s => s.ArtistTitle, StringComparer.CurrentCultureIgnoreCase);
                    break;
                case SongsSorting.Album:
                    enumerable = this.collection.OrderBy(s => s.AlbumTitle, StringComparer.CurrentCultureIgnoreCase);
                    break;
                case SongsSorting.AlbumDescending:
                    enumerable = this.collection.OrderByDescending(s => s.AlbumTitle, StringComparer.CurrentCultureIgnoreCase);
                    break;
                case SongsSorting.Duration:
                    enumerable = this.collection.OrderBy(s => s.Duration);
                    break;
                case SongsSorting.DurationDescending:
                    enumerable = this.collection.OrderByDescending(s => s.Duration);
                    break;
                case SongsSorting.Rating:
                    enumerable = this.collection.OrderBy(s => s.Rating);
                    break;
                case SongsSorting.RatingDescending:
                    enumerable = this.collection.OrderByDescending(s => s.Rating);
                    break;
                case SongsSorting.PlaysCount:
                    enumerable = this.collection.OrderBy(s => s.PlayCount);
                    break;
                case SongsSorting.PlaysCountDescending:
                    enumerable = this.collection.OrderByDescending(s => s.PlayCount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return enumerable;
        }

        private void RateSong(object parameter)
        {
            var ratingEventArgs = parameter as RatingEventArgs;
            Debug.Assert(ratingEventArgs != null, "ratingEventArgs != null");
            if (ratingEventArgs != null)
            {
                this.logger.LogTask(this.songsService.UpdateRatingAsync(
                        ((SongBindingModel)ratingEventArgs.CommandParameter).Metadata, (byte)ratingEventArgs.Value));
            }
        }

        private void SelectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.selectedObjectsService.Update(
                e.NewItems == null ? null : e.NewItems.Cast<SongBindingModel>().Select(x => x.Metadata),
                e.OldItems == null ? null : e.OldItems.Cast<SongBindingModel>().Select(x => x.Metadata));
        }
    }
}
