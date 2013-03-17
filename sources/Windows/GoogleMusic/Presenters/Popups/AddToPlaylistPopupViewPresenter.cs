// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Presenters;

    public class AddToPlaylistPopupViewPresenter : ViewPresenterBase<IAddToPlaylistPopupView>
    {
        public class AddToSongMusicPlaylist
        {
            public AddToSongMusicPlaylist(
                UserPlaylistBindingModel userPlaylist,
                IEnumerable<SongBindingModel> addingSongs)
            {
                this.Playlist = userPlaylist;
                this.SongContainsCount = addingSongs.Count(x => this.Playlist.Songs.Contains(x));
            }

            public UserPlaylistBindingModel Playlist { get; set; }

            public int SongContainsCount { get; set; }
        }

        private readonly IPlaylistCollectionsService collectionsService;
        private readonly IUserPlaylistRepository userPlaylistRepository;

        private bool isLoading;

        private List<AddToSongMusicPlaylist> playlists;

        public AddToPlaylistPopupViewPresenter(
            IEnumerable<SongBindingModel> songs,
            IPlaylistCollectionsService collectionsService,
            IUserPlaylistRepository userPlaylistRepository)
        {
            this.Songs = songs.ToList();
            this.collectionsService = collectionsService;
            this.userPlaylistRepository = userPlaylistRepository;
        }

        public bool IsLoading
        {
            get
            {
                return this.isLoading;
            }

            set
            {
                this.isLoading = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public List<AddToSongMusicPlaylist> Playlists
        {
            get
            {
                return this.playlists;
            }

            set
            {
                this.playlists = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public List<SongBindingModel> Songs { get; set; }

        public void AddToPlaylist(AddToSongMusicPlaylist playlist)
        {
            this.Logger.LogTask(this.userPlaylistRepository.AddEntriesAsync(playlist.Playlist.Metadata, this.Songs));
            this.View.Close();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.IsLoading = true;

            this.Logger.LogTask(Task.Run(async () =>
                {
                    var result = (await this.collectionsService
                        .GetCollection<UserPlaylistBindingModel>()
                        .GetAllAsync(Order.Name))
                        .Select(x => new AddToSongMusicPlaylist(x, this.Songs))
                        .ToList();

                    await this.Dispatcher.RunAsync(() => this.Playlists = result);
                    await this.Dispatcher.RunAsync(() => this.IsLoading = false);
                }));
        }
    }
}