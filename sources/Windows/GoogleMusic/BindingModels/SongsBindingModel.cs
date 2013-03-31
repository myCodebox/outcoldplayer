// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;

    using OutcoldSolutions.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;

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

    public class SongsBindingModel : BindingModelBase
    {
        private readonly IPlayQueueService playQueueService;

        private readonly IDispatcher dispatcher;

        private ObservableCollection<SongBindingModel> songs;
        private SongsSorting currentSorting = SongsSorting.Unknown;

        public SongsBindingModel(
            IPlayQueueService playQueueService,
            IDispatcher dispatcher)
        {
            this.playQueueService = playQueueService;
            this.dispatcher = dispatcher;
            this.Songs = new ObservableCollection<SongBindingModel>();
            this.SortCommand = new DelegateCommand(this.SortSongs);
            this.SelectedItems = new ObservableCollection<SongBindingModel>();

            this.playQueueService.StateChanged += this.PlayQueueServiceOnStateChanged;
        }

        public DelegateCommand SortCommand { get; set; }

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

        public void SetCollection(IEnumerable<Song> collection)
        {
            if (collection == null)
            {
                this.Songs = null;
            }
            else
            {
                this.Songs = new ObservableCollection<SongBindingModel>(collection.Select(s => new SongBindingModel(s)));
            }

            this.CurrentSorting = SongsSorting.Unknown;
            this.SelectedItems.Clear();
            this.UpdateSongStates(this.playQueueService.GetCurrentSong(), this.playQueueService.State);
        }

        public IEnumerable<Song> GetSelectedSongs()
        {
            return this.SelectedItems.Select(bm => bm.Metadata);
        }

        public IEnumerable<int> GetSelectedIndexes()
        {
            return this.SelectedItems.Select(bm => this.Songs.IndexOf(bm));
        }

        public void SelectSongByIndex(int index)
        {
            if (this.Songs.Count <= index)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (index == -1)
            {
                this.SelectedItems.Clear();
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

        private async void PlayQueueServiceOnStateChanged(object sender, StateChangedEventArgs stateChangedEventArgs)
        {
            await this.dispatcher.RunAsync(
                () => this.UpdateSongStates(stateChangedEventArgs.CurrentSong, stateChangedEventArgs.State));
        }

        private void UpdateSongStates(Song currentSong, QueueState queueState)
        {
            foreach (var song in this.Songs)
            {
                if (currentSong != null && song.Metadata.SongId == currentSong.SongId)
                {
                    if (queueState == QueueState.Play)
                    {
                        song.State = SongState.Playing;
                    }
                    else if (queueState == QueueState.Paused)
                    {
                        song.State = SongState.Paused;
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

        private void SortSongs(object obj)
        {
            Debug.Assert(obj is SongsSorting, "obj is SongsSorting");
            if (obj is SongsSorting && this.Songs != null)
            {
                IEnumerable<SongBindingModel> enumerable;

                var songsSorting = (SongsSorting)obj;
                switch (songsSorting)
                {
                    case SongsSorting.Unknown:
                        enumerable = this.Songs;
                        break;
                    case SongsSorting.Track:
                        enumerable = this.Songs.OrderBy(s => s.Metadata.Track);
                        break;
                    case SongsSorting.TrackDescending:
                        enumerable = this.Songs.OrderByDescending(s => s.Metadata.Track);
                        break;
                    case SongsSorting.Title:
                        enumerable = this.Songs.OrderBy(s => s.Metadata.Title, StringComparer.CurrentCultureIgnoreCase);
                        break;
                    case SongsSorting.TitleDescending:
                        enumerable = this.Songs.OrderByDescending(s => s.Metadata.Title, StringComparer.CurrentCultureIgnoreCase);
                        break;
                    case SongsSorting.Artist:
                        enumerable = this.Songs.OrderBy(s => s.Metadata.Artist.Title, StringComparer.CurrentCultureIgnoreCase);
                        break;
                    case SongsSorting.ArtistDescending:
                        enumerable = this.Songs.OrderByDescending(s => s.Metadata.Artist.Title, StringComparer.CurrentCultureIgnoreCase);
                        break;
                    case SongsSorting.Album:
                        enumerable = this.Songs.OrderBy(s => s.Metadata.Album.Title, StringComparer.CurrentCultureIgnoreCase);
                        break;
                    case SongsSorting.AlbumDescending:
                        enumerable = this.Songs.OrderByDescending(s => s.Metadata.Album.Title, StringComparer.CurrentCultureIgnoreCase);
                        break;
                    case SongsSorting.Duration:
                        enumerable = this.Songs.OrderBy(s => s.Metadata.Duration);
                        break;
                    case SongsSorting.DurationDescending:
                        enumerable = this.Songs.OrderByDescending(s => s.Metadata.Duration);
                        break;
                    case SongsSorting.Rating:
                        enumerable = this.Songs.OrderBy(s => s.Metadata.Rating);
                        break;
                    case SongsSorting.RatingDescending:
                        enumerable = this.Songs.OrderByDescending(s => s.Metadata.Rating);
                        break;
                    case SongsSorting.PlaysCount:
                        enumerable = this.Songs.OrderBy(s => s.Metadata.PlayCount);
                        break;
                    case SongsSorting.PlaysCountDescending:
                        enumerable = this.Songs.OrderByDescending(s => s.Metadata.PlayCount);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                this.SelectedItems.Clear();
                this.Songs = new ObservableCollection<SongBindingModel>(enumerable);
                this.CurrentSorting = songsSorting;
            }
        }
    }
}
