// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Web;

    public class CurrentPlaylistViewPresenter : ViewPresenterBase<ICurrentPlaylistView>
    {
        private readonly ICurrentPlaylistService currentPlaylistService;

        private readonly ISongWebService songWebService;

        private readonly IPlaylistCollectionsService playlistCollectionsService;

        private readonly IMusicPlaylistRepository musicPlaylistRepository;

        public CurrentPlaylistViewPresenter(
            IDependencyResolverContainer container, 
            ICurrentPlaylistView view,
            ICurrentPlaylistService currentPlaylistService,
            ISongWebService songWebService,
            IPlaylistCollectionsService playlistCollectionsService,
            IMusicPlaylistRepository musicPlaylistRepository)
            : base(container, view)
        {
            this.currentPlaylistService = currentPlaylistService;
            this.songWebService = songWebService;
            this.playlistCollectionsService = playlistCollectionsService;
            this.musicPlaylistRepository = musicPlaylistRepository;
            this.BindingModel = new CurrentPlaylistBindingModel();

            this.currentPlaylistService.PlaylistChanged += (sender, args) => this.UpdateSongs();
            this.UpdateSongs(); 

            this.BindingModel.PlaySelectedSong = new DelegateCommand(this.PlaySelectedSong);
            this.BindingModel.RemoveSelectedSong = new DelegateCommand(this.RemoveSelectedSong);
            this.AddToPlaylistCommand = new DelegateCommand(this.AddToPlaylist);
        }

        public CurrentPlaylistBindingModel BindingModel { get; private set; }

        public DelegateCommand AddToPlaylistCommand { get; private set; }

        public void AddSelectedSongToPlaylist(MusicPlaylist playlist)
        {
            var song = this.BindingModel.Songs[this.View.SelectedSongIndex];
            this.musicPlaylistRepository.AddEntry(playlist.Id, song);
        }

        public void UpdateRating(Song song, int newValue)
        {
            song.Rating = newValue;
            this.songWebService.UpdateRatingAsync(song.GoogleMusicMetadata, newValue).ContinueWith(
                        t =>
                        {
                            if (t.IsCompleted && !t.IsFaulted && t.Result != null)
                            {
                                if (this.Logger.IsDebugEnabled)
                                {
                                    this.Logger.Debug("Rating update completed for song: {0}.", song.GoogleMusicMetadata.Id);
                                    foreach (var songUpdate in t.Result.Songs)
                                    {
                                        this.Logger.Debug("Song updated: {0}, Rate: {1}.", songUpdate.Id, songUpdate.Rating);
                                    }
                                }
                            }
                            else
                            {
                                this.Logger.Debug("Failed to update rating for song: {0}.", song.GoogleMusicMetadata.Id);
                                if (t.IsFaulted && t.Exception != null)
                                {
                                    this.Logger.LogErrorException(t.Exception);
                                }
                            }
                        });
        }

        private void RemoveSelectedSong()
        {
            var selectedSongIndex = this.View.SelectedSongIndex;
            if (selectedSongIndex >= 0)
            {
                this.currentPlaylistService.RemoveAsync(selectedSongIndex)
                    .ContinueWith(
                        (t) =>
                        {
                            if (selectedSongIndex < this.BindingModel.Songs.Count)
                            {
                                this.View.SelectedSongIndex = selectedSongIndex;
                            }
                            else if (this.BindingModel.Songs.Count > 0)
                            {
                                this.View.SelectedSongIndex = selectedSongIndex - 1;
                            }
                        }, 
                        TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void PlaySelectedSong()
        {
            var selectedSongIndex = this.View.SelectedSongIndex;
            if (selectedSongIndex >= 0)
            {
                this.currentPlaylistService.PlayAsync(selectedSongIndex);
            }
        }

        private void UpdateSongs()
        {
            this.BindingModel.Songs.Clear();
            foreach (var song in this.currentPlaylistService.GetPlaylist())
            {
                this.BindingModel.Songs.Add(song);
            }
        }

        private void AddToPlaylist()
        {
            this.playlistCollectionsService.GetCollection<MusicPlaylist>().GetAllAsync(Order.Name).ContinueWith(
                t =>
                {
                    if (t.IsCompleted && !t.IsFaulted)
                    {
                        this.View.ShowPlaylists(t.Result.ToList());
                    }
                },
                TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}