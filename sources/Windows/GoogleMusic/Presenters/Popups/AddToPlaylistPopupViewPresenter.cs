// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Presenters;

    public class AddToPlaylistPopupViewPresenter : ViewPresenterBase<IAddToPlaylistPopupView>
    {
        public class AddToSongMusicPlaylist
        {
            public AddToSongMusicPlaylist(
                MusicPlaylist musicPlaylist,
                IEnumerable<Song> addingSongs)
            {
                this.Playlist = musicPlaylist;
                this.SongContainsCount = addingSongs.Count(x => this.Playlist.Songs.Contains(x));
            }

            public MusicPlaylist Playlist { get; set; }

            public int SongContainsCount { get; set; }
        }

        private readonly IPlaylistCollectionsService collectionsService;
        private readonly IMusicPlaylistRepository musicPlaylistRepository;

        private bool isLoading;

        private List<AddToSongMusicPlaylist> playlists;

        public AddToPlaylistPopupViewPresenter(
            IEnumerable<Song> songs,
            IDependencyResolverContainer container,
            IPlaylistCollectionsService collectionsService,
            IMusicPlaylistRepository musicPlaylistRepository)
            : base(container)
        {
            this.Songs = songs.ToList();
            this.collectionsService = collectionsService;
            this.musicPlaylistRepository = musicPlaylistRepository;
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

        public List<Song> Songs { get; set; }

        public void AddToPlaylist(AddToSongMusicPlaylist playlist)
        {
            this.Logger.LogTask(this.musicPlaylistRepository.AddEntriesAsync(playlist.Playlist.Id, this.Songs));
            this.View.Close();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.IsLoading = true;

            this.Logger.LogTask(Task.Run(async () =>
                {
                    var result = (await this.collectionsService
                        .GetCollection<MusicPlaylist>()
                        .GetAllAsync(Order.Name))
                        .Select(x => new AddToSongMusicPlaylist(x, this.Songs))
                        .ToList();

                    await this.Dispatcher.RunAsync(() => this.Playlists = result);
                    await this.Dispatcher.RunAsync(() => this.IsLoading = false);
                }));
        }
    }
}