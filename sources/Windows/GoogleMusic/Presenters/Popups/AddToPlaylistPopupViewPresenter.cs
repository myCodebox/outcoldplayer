// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System;
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
        private readonly IUserPlaylistsService userPlaylistsService;
        private readonly IUserPlaylistsRepository userPlaylistsRepository;

        private bool isLoading;

        private List<AddToSongMusicPlaylist> playlists;

        public AddToPlaylistPopupViewPresenter(
            IEnumerable<Song> songs,
            IUserPlaylistsService userPlaylistsService,
            IUserPlaylistsRepository userPlaylistsRepository)
        {
            this.Songs = songs.ToList();
            this.userPlaylistsService = userPlaylistsService;
            this.userPlaylistsRepository = userPlaylistsRepository;
        }

        public bool IsLoading
        {
            get
            {
                return this.isLoading;
            }

            set
            {
                this.SetValue(ref this.isLoading, value);
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
                this.SetValue(ref this.playlists, value);
            }
        }

        public List<Song> Songs { get; set; }

        public void AddToPlaylist(AddToSongMusicPlaylist playlist)
        {
            this.Logger.LogTask(this.userPlaylistsService.AddSongsAsync(playlist.Playlist, this.Songs));
            this.View.Close(new AddToPlaylistCompletedEventArgs());
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.IsLoading = true;

            this.Logger.LogTask(Task.Run(async () =>
                {
                    var songsWithEntries = await Task.WhenAll(this.Songs.Select(async x => Tuple.Create(x, await this.userPlaylistsRepository.GetAllSongEntriesAsync(x.SongId))).ToList());
                    var result = (await this.userPlaylistsRepository.GetAllAsync(Order.Name)).Select(x => new AddToSongMusicPlaylist(x, songsWithEntries)).ToList();

                    await this.Dispatcher.RunAsync(() => this.Playlists = result);
                    await this.Dispatcher.RunAsync(() => this.IsLoading = false);
                }));
        }

        public class AddToSongMusicPlaylist
        {
            public AddToSongMusicPlaylist(
                UserPlaylist userPlaylist,
                IEnumerable<Tuple<Song, IList<UserPlaylistEntry>>> addingSongs)
            {
                this.Playlist = userPlaylist;
                this.SongContainsCount = addingSongs.Count(x => x.Item2.Any(e => e.PlaylistId == userPlaylist.PlaylistId));
            }

            public UserPlaylist Playlist { get; set; }

            public int SongContainsCount { get; set; }
        }
    }
}